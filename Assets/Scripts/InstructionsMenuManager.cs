using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Management;

public class InstructionsMenuManager : MonoBehaviour
{
    XRLoader m_SelectedXRLoader;

    public void Awake()
    {
        //PopulateXRLoaders();
    }

    public void Next()
    {        
        SceneManager.LoadScene("Practice");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PopulateXRLoaders()
    {
        Dropdown loaderDrop = GameObject.Find("XRLoadersDropdown").GetComponent<Dropdown>();

        List<Dropdown.OptionData> loaderList = new List<Dropdown.OptionData>();

        foreach (var loader in XRGeneralSettings.Instance.Manager.activeLoaders)
        {
            loaderList.Add(new Dropdown.OptionData(loader.name));
        }

        loaderDrop.options = loaderList;
    }

    public void InitXR()
    {
        //if (m_SelectedXRLoader != null)
            StartCoroutine(StartXRCoroutine());
    }

    //IEnumerator StartXRCoroutine()
    //{
    //    Debug.Log("Init XR loader");
    //
    //    var initSuccess = m_SelectedXRLoader.Initialize();
    //    if (!initSuccess)
    //    {
    //        Debug.LogError("Error initializing selected loader.");
    //    }
    //    else
    //    {
    //        yield return null;
    //        Debug.Log("XR loader initialized!");
    //        Debug.Log("Start XR loader");
    //        var startSuccess = m_SelectedXRLoader.Start();
    //        if (!startSuccess)
    //        {
    //            yield return null;
    //            Debug.LogError("Error starting selected loader.");
    //            m_SelectedXRLoader.Deinitialize();
    //        }
    //
    //        GameObject.Find("ProceedButton").SetActive(true);
    //        this.gameObject.SetActive(false);
    //    }
    //}

    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }

    void StopXR()
    {
        Debug.Log("Stopping XR...");

        XRGeneralSettings.Instance.Manager.StopSubsystems();
        XRGeneralSettings.Instance.Manager.DeinitializeLoader();
        Debug.Log("XR stopped completely.");
    }
}
