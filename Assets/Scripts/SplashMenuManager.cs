using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (PlayerPrefs.GetInt("uploaded") == 1)
        {
            GameObject ub = GameObject.Find("UploadButton");
            ub.GetComponentInChildren<Text>().text = "Results Uploaded!";
            ub.GetComponent<Button>().interactable = false;

            GameObject.Find("CloseButton").GetComponent<Button>().interactable = true;
        }
        else
        {
            GameObject.Find("CloseButton").GetComponent<Button>().interactable = false;
        }
    }

    public void Proceed()
    {
        SceneManager.LoadScene("Intro");
    }

    public void UploadResults()
    {
        //Globals.resultsForm = new WWWForm();
        //
        //Globals.resultsForm.AddField("data", "some test data");
        //Globals.resultsForm.AddField("meta", "some metadata for testing");

        var sender = new SendStudyResults();
        StartCoroutine(sender.Upload());
    }

    public void End()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
