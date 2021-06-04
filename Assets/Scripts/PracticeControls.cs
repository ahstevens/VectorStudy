#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class PracticeControls : MonoBehaviour
{    
    public GameObject stimulus;
    public GameObject probe;

    public GameObject instructionText;
    public GameObject promptText;

    public float stimulusTime = 10f;

    public float keyboardSamplingRate = 0.01f;

    private bool trialActive = false;
    
    private bool spaceJustPressed = false;

    private int numPractices;

#if ENABLE_INPUT_SYSTEM
    InputAction probeResizeAction;

    void Start()
    {        
        mapActions();

        numPractices = 0;

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;
        instructionText.GetComponent<Renderer>().enabled = false;
        promptText.GetComponent<Renderer>().enabled = true;
    }
#endif

    // Update is called once per frame
    void Update()
    {
        if (IsSpacebarPressed() && !trialActive)
        {
            StartCoroutine(PerformTrial());
        }

        if (numPractices >= 5 && Keyboard.current != null && Keyboard.current.enterKey.IsPressed())
            SceneManager.LoadScene("Study");
    }

    void mapActions()
    {
        var map = new InputActionMap("Practice Controls");

        probeResizeAction = map.AddAction("Resize Probe", binding: "<Mouse>/scroll");
        probeResizeAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow");
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
        float tt = 0;

        trialActive = true;

        probe.transform.localScale = new Vector3(0.003f, 0.005f, 0.003f);

        stimulus.transform.localScale = new Vector3(stimulus.transform.localScale.x, Random.Range(0.05f, 0.25f) * 0.5f, stimulus.transform.localScale.z);
        stimulus.transform.localEulerAngles = new Vector3(stimulus.transform.localEulerAngles.x, Random.Range(0, 360), stimulus.transform.localEulerAngles.z);
                  
        probe.GetComponent<Renderer>().enabled = true;
        stimulus.GetComponent<Renderer>().enabled = true;
        promptText.GetComponent<Renderer>().enabled = false;
        instructionText.GetComponent<Renderer>().enabled = true;

        probeResizeAction.Enable();

        while (tt < 1)
        {

            float probeMove = probeResizeAction.ReadValue<Vector2>().y;

            if (probeMove != 0f)
                probe.transform.localScale = new Vector3(probe.transform.localScale.x, probe.transform.localScale.y + 0.0005f * Mathf.Sign(probeMove), probe.transform.localScale.z);
            
            if (probe.transform.localScale.y < 0.0005f)
                probe.transform.localScale = new Vector3(probe.transform.localScale.x, 0.0005f, probe.transform.localScale.z);

            yield return null;

            tt += Time.deltaTime / stimulusTime;
        }

        probeResizeAction.Disable();

        numPractices++;

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;

        float probeLength = probe.transform.localScale.y;
        float targetLength = stimulus.transform.localScale.y;

        promptText.GetComponent<TMPro.TextMeshPro>().text = "Last Trial Match Error:\n(lower is better)\n" + (((targetLength - probeLength) / targetLength) * 100f).ToString("0.0") + "%\n\nPress the Space Bar\non your keyboard\nto begin a practice trial.";
        if (numPractices >= 5)
            promptText.GetComponent<TMPro.TextMeshPro>().text += "\n\nPress the Enter Key\non your keyboard\nto begin the study.";
        promptText.GetComponent<Renderer>().enabled = true;
        instructionText.GetComponent<Renderer>().enabled = false;

        trialActive = false;
    }
}