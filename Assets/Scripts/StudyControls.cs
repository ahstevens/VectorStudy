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
using UnityEngine.SceneManagement;

public class StudyControls : MonoBehaviour
{
    static StudyCondition[] conditions = new StudyCondition[48];

    static float[] results = new float[48];

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
            var replications = 2;
            foreach (float r in glyphAngles)
                foreach (float l in glyphLengths)
                    for (int i = 0; i < replications; ++i)
                        conditionQueue.Add(new StudyCondition(r, l));

            conditionQueue.Shuffle();

            int ind = 0;
            foreach (StudyCondition c in conditionQueue)            
                conditions[ind++] = c;
            
        }

        public bool incrementCondition()
        {
            currentCondition++;

            return isActive();
        }

        public void loadStimulusCondition(GameObject stimulus)
        {
            stimulus.transform.localScale = new Vector3(stimulus.transform.localScale.x, conditions[currentCondition].glyphLength * 0.5f, stimulus.transform.localScale.z);
            stimulus.transform.localEulerAngles = new Vector3(90, conditions[currentCondition].glyphAngle, 90);
           
        }

        public int getCurrentTrialNumber()
        {
            return currentCondition;
        }

        public void recordState(GameObject probe)
        {
            if (isActive())
                results[currentCondition] = probe.transform.localScale.y * 2f;
        }
    }

    public GameObject stimulus;
    public GameObject probe;

    public GameObject stareText;
    public GameObject statusText;

    public float blankingTime = 3.5f;
    public float stimulusTime = 10f;
    public float keyboardSamplingRate = 0.01f;

    private StudyState studyState;

    private bool trialActive = false;

    private bool endOfStudy = false;

    void Start()
    {
        studyState = new StudyState();
        
        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (studyState.isActive() && !trialActive)
            StartCoroutine(PerformTrial());

        if (studyState.isDone() && !trialActive && !endOfStudy)
        {
            StartCoroutine(SaveResultsToFile());

            var sender = new SendStudyResults();
            sender.Prepare(conditions, results);
            StartCoroutine(sender.Upload());

            StartCoroutine(EndStudy());

            endOfStudy = true;
        }
    }

    IEnumerator PerformTrial()
    {
        float bt = 0;
        float mt = 0;
        float tt = 0;


        if (!studyState.incrementCondition())
            yield break;

        trialActive = true;


        statusText.SetActive(true);
        statusText.GetComponent<TMPro.TextMeshPro>().text = "Trial " + (studyState.getCurrentTrialNumber() + 1) + " of 48";

        while (bt < 1)
        {
            yield return null;

            if (mt < 1)
                mt += Time.deltaTime / (blankingTime - 0.5f);
            else
                statusText.SetActive(false);

            bt += Time.deltaTime / blankingTime;
        }

        Transform hmdAtStart = this.gameObject.transform;

        Vector3 forwardFromHMD = Vector3.Cross(hmdAtStart.localToWorldMatrix.GetColumn(0), Vector3.up).normalized;

        GameObject probePivot = GameObject.Find("Pivot Point");

        probePivot.transform.position = hmdAtStart.position + forwardFromHMD * 0.57f;// + Vector3.up * 0.056f;

        probe.transform.localScale = new Vector3(0.003f, 0.005f, 0.003f);

        studyState.loadStimulusCondition(stimulus);
        stimulus.transform.position = probePivot.transform.position + Vector3.up * 0.028f;
        //stimulus.transform.rotation = hmdAtStart.rotation * stimulus.transform.rotation;
        stimulus.transform.rotation = Quaternion.Euler(stimulus.transform.eulerAngles.x, hmdAtStart.eulerAngles.y + stimulus.transform.eulerAngles.y, stimulus.transform.eulerAngles.z);

        GameObject light = GameObject.Find("Directional Light");

        light.transform.position = hmdAtStart.position + Vector3.Cross(Vector3.up, forwardFromHMD) + Vector3.up - forwardFromHMD;
        light.transform.rotation = Quaternion.Euler(45, hmdAtStart.eulerAngles.y - 45, 0);

        probe.GetComponent<Renderer>().enabled = true;
        stimulus.GetComponent<Renderer>().enabled = true;
        
        while (tt < 1)
        {
            int probeMove = 0;

            var kb = Keyboard.current;

            if (kb.upArrowKey.isPressed || kb.leftArrowKey.isPressed || kb.wKey.isPressed || kb.dKey.isPressed)
                probeMove = 1;

            if (kb.downArrowKey.isPressed || kb.rightArrowKey.isPressed || kb.sKey.isPressed || kb.aKey.isPressed)
                probeMove -= 1;

            if (probeMove != 0f)
                probe.transform.localScale = new Vector3(probe.transform.localScale.x, probe.transform.localScale.y + 0.0005f * Mathf.Sign(probeMove), probe.transform.localScale.z);
        
            if (probe.transform.localScale.y < 0.0005f)
                probe.transform.localScale = new Vector3(probe.transform.localScale.x, 0.0005f, probe.transform.localScale.z);

            if (probe.transform.localPosition.y != -probe.transform.localScale.y)
                probe.transform.localPosition = new Vector3(probe.transform.localPosition.x, -probe.transform.localScale.y, probe.transform.localPosition.z);

            yield return null;

            tt += Time.deltaTime / stimulusTime;
        }

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;

        studyState.recordState(probe);

        trialActive = false;
    }

    private IEnumerator SaveResultsToFile()
    {
        string participant = PlayerPrefs.GetString("participant");

        //Path of the file
        string path = Application.dataPath + "/../" + participant + "_results.csv";

        //Create File if it doesn't exist
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "id,trial,ipd,view.dist,view.angle,view.dist.factor,fishtank,rod.angle,rod.length,response\n");

            for (int i = 0; i < 48; ++i)
            {
                string result = participant + "," + i + ",0,57,0,1,1," + conditions[i].glyphAngle + "," + conditions[i].glyphLength * 100 + "," + results[i] * 100f + "\n";
                File.AppendAllText(path, result);
            }

        }

        File.WriteAllText(Application.dataPath + "/../" + participant + "_metadata.txt", participant + "\n" + PlayerPrefs.GetString("age") + "\n" + PlayerPrefs.GetString("sex") + "\n" + PlayerPrefs.GetString("hmd"));

        yield return null;

        Debug.Log("Saved results to " + path);
    }

    public void StartStudy()
    {
        StartCoroutine(PerformTrial());
    }

    private IEnumerator EndStudy()
    {
        float ft = 0;

        stareText.SetActive(true);
        stareText.GetComponent<TMPro.TextMeshPro>().text = "Complete!\n\nPlease remove your HMD.";

        while (ft < 1)
        {
            yield return null;
            ft += Time.deltaTime / 5f;
        }

        ManualXRControl xr = new ManualXRControl();
        xr.StopXR();

        SceneManager.LoadScene("Ending");
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