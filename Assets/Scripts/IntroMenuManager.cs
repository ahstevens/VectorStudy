using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroMenuManager : MonoBehaviour
{    
    public void Next()
    {
        var name = GameObject.Find("ParticipantName").GetComponent<InputField>().text;
        var age = GameObject.Find("ParticipantAge").GetComponent<InputField>().text;
        var maleChecked = GameObject.Find("MaleCheckbox").GetComponent<Toggle>().isOn;
        var femaleChecked = GameObject.Find("FemaleCheckbox").GetComponent<Toggle>().isOn;

        var dropdownObject = GameObject.Find("HMDDropdown").GetComponent<Dropdown>();
        var hmd = dropdownObject.options[dropdownObject.value].text;        

        if (name.Length == 0)
        {
            GameObject.Find("NamePrompt").GetComponent<Text>().color = Color.red;
            return;
        }
        else
        {
            GameObject.Find("NamePrompt").GetComponent<Text>().color = Color.black;
        }

        if (age.Length == 0)
        {
            GameObject.Find("AgePrompt").GetComponent<Text>().color = Color.red;
            return;
        }
        else
        {
            GameObject.Find("AgePrompt").GetComponent<Text>().color = Color.black;
        }

        if (!maleChecked && !femaleChecked)
        {
            GameObject.Find("MaleCheckbox").GetComponentInChildren<Text>().color = Color.red;
            GameObject.Find("FemaleCheckbox").GetComponentInChildren<Text>().color = Color.red;
            return;
        }

        if (hmd == "<Select Your HMD>")
        {
            GameObject.Find("HMDDropdown").GetComponentInChildren<Text>().color = Color.red;
            return;
        }

        PlayerPrefs.SetString("participant", name);
        PlayerPrefs.SetString("age", age);
        PlayerPrefs.SetString("sex", maleChecked ? "m" : "f");
        PlayerPrefs.SetString("hmd", hmd);
        SceneManager.LoadScene("Instructions");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
