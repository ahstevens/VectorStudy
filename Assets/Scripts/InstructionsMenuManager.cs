using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class InstructionsMenuManager : MonoBehaviour
{
    private GameObject xrButton;
    private GameObject proceedButton;
    private ManualXRControl xr;

    public void Start()
    {
        xr = new ManualXRControl();

        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            xr.StopXR();

        xrButton = GameObject.Find("InitXRButton");
        proceedButton = GameObject.Find("ProceedButton");
    }

    public void Update()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            proceedButton.GetComponent<Button>().interactable = false;
            xrButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            proceedButton.GetComponent<Button>().interactable = true;
            xrButton.GetComponent<Button>().interactable = false;
            xrButton.GetComponentInChildren<Text>().text = "OpenXR Initialized";
        }
    }

    public void Next()
    {        
        SceneManager.LoadScene("Practice");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void InitXR()
    {
        xrButton.GetComponentInChildren<Text>().text = "Initializing...";
        StartCoroutine(xr.StartXRCoroutine());
    }

    public void OnApplicationQuit()
    {
        if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            xr.StopXR();
    }
}
