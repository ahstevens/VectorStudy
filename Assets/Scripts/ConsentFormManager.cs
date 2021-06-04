using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ConsentFormManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("ConsentButton").GetComponent<Button>().interactable = false;
        GameObject.Find("ConsentText").GetComponent<Text>().text = "Please read the entire consent form before proceeding.";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void checkConsentScroll()
    {
        if (GameObject.Find("Scrollbar Vertical").GetComponent<Scrollbar>().value <= 0.05F)
        {
            GameObject.Find("ConsentButton").GetComponent<Button>().interactable = true;
            GameObject.Find("ConsentText").GetComponent<Text>().text = "Yes, I, " + PlayerPrefs.GetString("participant") + ", consent/agree to participate in this research project.";
        }
    }

    public void Consent()
    {
        SceneManager.LoadScene("Instructions");
    }

    public void Decline()
    {
        Application.Quit();
    }
}
