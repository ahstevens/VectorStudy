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

    private int numPractices;

    void Start()
    {
        numPractices = 0;

        probe.GetComponent<Renderer>().enabled = false;
        stimulus.GetComponent<Renderer>().enabled = false;
        instructionText.GetComponent<Renderer>().enabled = false;
        promptText.GetComponent<Renderer>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && !trialActive)
        {
            StartCoroutine(PerformTrial());
        }

        if (numPractices >= 5 && Keyboard.current != null && Keyboard.current.enterKey.IsPressed())
            SceneManager.LoadScene("Study");
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

            yield return null;

            tt += Time.deltaTime / stimulusTime;
        }

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