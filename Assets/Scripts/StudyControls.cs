#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.XR.Management;

public class StudyControls : MonoBehaviour
{
    static StudyCondition[] conditions = new StudyCondition[24];

    static float[] results = new float[24];

    class StudyState
    {
        List<StudyCondition> conditionQueue = new List<StudyCondition>();

        int currentCondition;

        public StudyState()
        {
            makeConditions();

            currentCondition = -1;
        }

        public bool isActive()
        {
            return currentCondition >= 0 && currentCondition < conditionQueue.Count;
        }

        public bool isDone()
        {
            return currentCondition == conditionQueue.Count;
        }

        void makeConditions()
        {
            var glyphAngles = new List<float> { 0f, 15f, 30f, 45f, 60f, 75f, 90f, 105f, 120f, 135f, 150f, 165f };
            var glyphLengths = new List<float> { 0.1f, 0.2f };
            foreach (float r in glyphAngles)
                foreach (float l in glyphLengths)
                    conditionQueue.Add(new StudyCondition(r, l));

            conditionQueue.Shuffle();

            int ind = 0;
            foreach (StudyCondition c in conditionQueue)            
                conditions[ind++] = c;
            
        }

        public bool loadStimulusCondition(GameObject stimulus)
        {
            currentCondition++;

            if (isActive())
            {
                stimulus.transform.localScale = new Vector3(stimulus.transform.localScale.x, conditions[currentCondition].glyphLength * 0.5f, stimulus.transform.localScale.z);
                stimulus.transform.localEulerAngles = new Vector3(stimulus.transform.localEulerAngles.x, conditions[currentCondition].glyphAngle, stimulus.transform.localEulerAngles.z);
                
                return true;
            }

            return false;
        }

        public void recordState(GameObject probe)
        {
            if (isActive())
                results[currentCondition] = probe.transform.localScale.y * 2f;
        }
    }

    public GameObject stimulus;
    public GameObject probe;

    public float blankingTime = 3.5f;
    public float stimulusTime = 10f;

    private StudyState studyState;

    private bool trialActive = false;
    
    private bool spaceJustPressed = false;

#if ENABLE_INPUT_SYSTEM
    InputAction probeResizeAction;

    public void Awake()
    {
        XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }

    private void OnApplicationQuit()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
        {
            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        }
    }

    void Start()
    {
        studyState = new StudyState();

        mapActions();

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;
                
        Debug.Log(XRSettings.loadedDeviceName);

        var inputDevices = new List<UnityEngine.XR.InputDevice>();
        UnityEngine.XR.InputDevices.GetDevices(inputDevices);

        foreach (var device in inputDevices)
        {
            Debug.Log(string.Format("Device found with name '{0}' and role '{1}'", device.name, device.characteristics.ToString()));
        }
    }
#endif

    // Update is called once per frame
    void Update()
    {
        // Exit Sample  

        if (IsEscapePressed())
        {            
//            Application.Quit();
//#if UNITY_EDITOR
//            UnityEditor.EditorApplication.isPlaying = false;
//#endif
        }

        if (IsSpacebarPressed())
        {
            var sender = new SendStudyResults();
            StartCoroutine(sender.Upload(conditions, results));
        }

        if (studyState.isActive() && !trialActive)
            StartCoroutine(PerformTrial());

        if (studyState.isDone())
        {
            StartCoroutine(_SaveResultsToFile());
            //StartCoroutine(_UploadResults());
            Application.Quit();
        }
    }

    void mapActions()
    {
        var map = new InputActionMap("Study Controls");

        probeResizeAction = map.AddAction("Resize Probe", binding: "<Mouse>/scroll");
        probeResizeAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow");
    }

    bool IsEscapePressed()
    {
        return Keyboard.current != null ? Keyboard.current.escapeKey.isPressed : false;
    }

    bool IsSpacebarPressed()
    {
        if (Keyboard.current == null)
            return false;

        if (Keyboard.current.spaceKey.isPressed && !spaceJustPressed)
        {
            spaceJustPressed = true;
            return true;
        }

        if (spaceJustPressed && !Keyboard.current.spaceKey.isPressed)
            spaceJustPressed = false;

        return false;
    }

    IEnumerator PerformTrial()
    {
        float bt = 0;
        float tt = 0;

        trialActive = true;

        studyState.loadStimulusCondition(stimulus);
        probe.transform.localScale = new Vector3(0.003f, 0.005f, 0.003f);

        while (bt < 1)
        {
            yield return null;

            bt += Time.deltaTime / blankingTime;
        }        

        probe.GetComponent<Renderer>().enabled = true;
        stimulus.GetComponent<Renderer>().enabled = true;
        
        probeResizeAction.Enable();

        while (tt < 1)
        {
            float probeMove = probeResizeAction.ReadValue<Vector2>().y;

            if (probeMove != 0f)
                probe.transform.localScale = new Vector3(probe.transform.localScale.x, probe.transform.localScale.y + 0.0005f * Mathf.Sign(probeMove), probe.transform.localScale.z);

            if (probe.transform.localPosition.y != -probe.transform.localScale.y)
                probe.transform.localPosition = new Vector3(probe.transform.localPosition.x, -probe.transform.localScale.y, probe.transform.localPosition.z);

            yield return null;

            tt += Time.deltaTime / stimulusTime;
        }

        probeResizeAction.Disable();

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;

        studyState.recordState(probe);

        trialActive = false;
    }

    private IEnumerator _SaveResultsToFile()
    {
        //Path of the file
        string path = Application.dataPath + "/results.csv";

        //Create File if it doesn't exist
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "id,trial,ipd,view.dist,view.angle,view.dist.factor,fishtank,rod.angle,rod.length,response\n");

            for (int i = 0; i < 24; ++i)
            {
                string result = "person," + i + ",0,57,0,1,1," + conditions[i].glyphAngle + "," + conditions[i].glyphLength * 100 + "," + results[i] * 100f + "\n";
                File.AppendAllText(path, result);
            }

        }

        yield return null;

        Debug.Log("Saved results to " + path);
    }

    public void StartStudy()
    {
        StartCoroutine(PerformTrial());
    }
}
static class CCOM
{
    private static System.Random rng = new System.Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}