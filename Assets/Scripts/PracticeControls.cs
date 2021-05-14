#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PracticeControls : MonoBehaviour
{    
    public GameObject stimulus;
    public GameObject probe;

    public float blankingTime = 3.5f;
    public float stimulusTime = 10f;

    private bool trialActive = false;
    
    private bool spaceJustPressed = false;

#if ENABLE_INPUT_SYSTEM
    InputAction probeResizeAction;

    void Start()
    {        
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

    IEnumerator PerformTrial()
    {
        float bt = 0;
        float tt = 0;

        trialActive = true;

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
        
        trialActive = false;
    }
}