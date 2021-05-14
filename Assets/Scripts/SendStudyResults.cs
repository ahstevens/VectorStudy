using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendStudyResults
{
    public string resultsUploadEndpoint = "http://192.168.1.234/results/upload.php";

    public IEnumerator Upload(StudyCondition[] conditions, float[] results)
    {
        string result = "id,trial,ipd,view.dist,view.angle,view.dist.factor,fishtank,rod.angle,rod.length,response\n";

        string pName = PlayerPrefs.GetString("participant");

        for (int i = 0; i < 24; ++i)
        {
            result += pName + "," + i + ",0,57,0,1,1," + conditions[i].glyphAngle + "," + conditions[i].glyphLength * 100 + "," + results[i] * 100f + "\n";
        }

        WWWForm form = new WWWForm();

        form.AddField("name", pName);
        form.AddField("data", result);

        UnityWebRequest www = UnityWebRequest.Post(resultsUploadEndpoint, form);

        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
            Debug.Log(form);
        }
        else
        {
            Debug.Log("Post request complete!" + " Response Code: " + www.responseCode);
            string responseText = www.downloadHandler.text;
            Debug.Log("Response Text:" + responseText);
        }
    }
}
