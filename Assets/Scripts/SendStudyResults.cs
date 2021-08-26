using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class Globals
{
    public static WWWForm resultsForm;
}

public class SendStudyResults
{
    public string resultsUploadEndpoint = "http://vislab-ccom.unh.edu/~drew/experiments/vectorlength/processdata.php";

    public void Prepare(StudyCondition[] conditions, float[] results)
    {
        string result = "id,trial,ipd,view.dist,view.angle,view.dist.factor,fishtank,rod.angle,rod.length,response\n";

        string pName = PlayerPrefs.GetString("participant");

        string metadata = PlayerPrefs.GetString("participant") + "\n" + PlayerPrefs.GetString("age") + "\n" + PlayerPrefs.GetString("sex") + "\n" + PlayerPrefs.GetString("hmd");

        for (int i = 0; i < 48; ++i)
        {
            result += pName + "," + i + ",0,57,0,1,1," + conditions[i].glyphAngle + "," + conditions[i].glyphLength * 100 + "," + results[i] * 100f + "\n";
        }

        Globals.resultsForm = new WWWForm();

        Globals.resultsForm.AddField("data", result);
        Globals.resultsForm.AddField("meta", metadata);

        PlayerPrefs.SetInt("uploaded", 0);
    }

    public IEnumerator Upload()
    {
        UnityWebRequest www = UnityWebRequest.Post(resultsUploadEndpoint, Globals.resultsForm);

        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(Globals.resultsForm);
        }
        else
        {
            Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
            string responseText = www.downloadHandler.text;
            Debug.Log("Response Text:" + responseText);

            if (responseText == "SUCCESS")
            {
                PlayerPrefs.SetInt("uploaded", 1);
            }
        }
    }
}
