using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StareTimer : MonoBehaviour
{
    public float textDistance = 3f;

    public float stareDuration;

    public string preText;
    public string postText;

    public float boxColliderSizeRatio = 0.5f;

    private float timeleft;

    private bool done;

    public float easingTime = 1;

    private IEnumerator currentEaseCoroutine;

    private Vector3 easingGazeAnchor;
    private Vector3 easingGazeTarget;
    private Vector3 easingAngle;

    EasingFunction.Function easeFunc;

    // Start is called before the first frame update
    void Start()
    {
        timeleft = stareDuration;
        done = false;

        currentEaseCoroutine = null;

        easingGazeAnchor = Vector3.zero;
        easingGazeTarget = Vector3.zero;

        easeFunc = EasingFunction.GetEasingFunction(EasingFunction.Ease.EaseInOutCubic);
    }

    // Update is called once per frame
    void Update()
    {

        Ray gaze = new Ray(Camera.main.transform.position, Camera.main.transform.localToWorldMatrix.GetColumn(2));

        Vector3 forward = Vector3.Cross(Camera.main.transform.localToWorldMatrix.GetColumn(0), Vector3.up).normalized;

        if (easingGazeAnchor == Vector3.zero)
        {
            easingGazeAnchor = forward;
            easingGazeTarget = easingGazeAnchor;
            transform.position = gaze.origin + forward * textDistance;
        }

        if (Mathf.Abs(Vector3.Angle(easingGazeTarget, forward)) > 30)
        {
            easingGazeTarget = forward;

            if (currentEaseCoroutine != null)
            {
                StopCoroutine(currentEaseCoroutine);
                easingGazeAnchor = easingAngle;
            }

            currentEaseCoroutine = EasingCoroutine(easingTime);

            StartCoroutine(currentEaseCoroutine);
        }

        //transform.position = gaze.origin + forward * 3f;

        Vector3 textToHMD = (transform.position - Camera.main.transform.position).normalized;

        Matrix4x4 billboardXform = Matrix4x4.identity;
        billboardXform.SetColumn(0, Vector3.Cross(Vector3.up, textToHMD));
        billboardXform.SetColumn(1, Vector3.up);
        billboardXform.SetColumn(2, textToHMD);
        billboardXform.SetRow(3, new Vector4(0,0,0,1));

        transform.rotation = billboardXform.rotation;

        if (!done)
        {
            RaycastHit hit;
            if (Physics.Raycast(gaze, out hit, Mathf.Infinity))
            {
                GetComponent<TMPro.TextMeshPro>().text = "Stare at this message for " + (timeleft + 0.5f).ToString("0") + " seconds to start.";
                
                timeleft -= Time.deltaTime;

                if (timeleft <= 0.0f)
                {
                    done = true;
                    GetComponent<TMPro.TextMeshPro>().text = postText;
                    StartCoroutine(FadeCoroutine(5));
                }
            }
            else
            {
                timeleft = stareDuration;
                GetComponent<TMPro.TextMeshPro>().text = preText;
            }

            GetComponent<BoxCollider>().size = new Vector3(GetComponent<RectTransform>().sizeDelta.x, GetComponent<RectTransform>().sizeDelta.y, 0) * boxColliderSizeRatio;
        }
    }

    IEnumerator FadeCoroutine(float fadeTime)
    {
        float waitTime = 0;
        while (waitTime < 1)
        {
            GetComponent<TMPro.TextMeshPro>().fontMaterial.SetColor("_FaceColor", Color.Lerp(Color.white, Color.clear, waitTime));
            yield return null;
            waitTime += Time.deltaTime / fadeTime;
        }

        GetComponent<TMPro.TextMeshPro>().fontMaterial.SetColor("_FaceColor", Color.white);
        gameObject.SetActive(false);

        Camera.main.gameObject.GetComponent<StudyControls>().StartStudy();
    }

    IEnumerator EasingCoroutine(float easeTime)
    {
        float waitTime = 0;
        while (waitTime < 1)
        {
            float rotAmt = easeFunc(0, 1, waitTime);
            easingAngle = Vector3.Lerp(easingGazeAnchor, easingGazeTarget, rotAmt);
            transform.position = Camera.main.transform.position + easingAngle * textDistance;
            yield return null;
            waitTime += Time.deltaTime / easeTime;

        }

    }
}
