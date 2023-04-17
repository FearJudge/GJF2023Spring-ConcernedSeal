using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* 
 * A class that handles a simple progression bar at the bottom of the screen.
 * Is a real pain in the ass.
 */
public class ProgressionScript : MonoBehaviour
{
    const float MINIMUMVALUECHANGE = 0.001f;
    const float SPEEDUPVALUEDIFFERENCE = 0.06f;
    const float SPEEDUPMAXVALUEDIFFERENCE = 0.13f;
    const float VALUECHANGESPEEDMULTSMALL = 0.04f;
    const float VALUECHANGESPEEDMULTBIG = 0.1f;

    public Slider playerProgressionSlider;
    public float targetValue = 0f;
    bool lockedCoroutine = false;

    // Start is called before the first frame update
    void Start()
    {
        if (playerProgressionSlider == null) { playerProgressionSlider = GetComponent<Slider>(); }
        FinishLine.PlayerProgressChanged += ChangePlayerProgress;
    }

    private void OnDestroy()
    {
        FinishLine.PlayerProgressChanged -= ChangePlayerProgress;
    }

    private void OnDisable()
    {
        FinishLine.PlayerProgressChanged -= ChangePlayerProgress;
    }

    private void OnEnable()
    {
        if (lockedCoroutine) { lockedCoroutine = false; FinishLine.PlayerProgressChanged += ChangePlayerProgress; }
    }

    void ChangePlayerProgress(float val)
    {
        targetValue = val;
        if (gameObject.activeSelf == false || playerProgressionSlider.enabled == false) { return; }
        if (targetValue == playerProgressionSlider.value) { return; }
        StartCoroutine(LerpToTargetValue(playerProgressionSlider));
    }

    IEnumerator LerpToTargetValue(Slider toTrack)
    {
        if (lockedCoroutine) { yield break; }
        lockedCoroutine = true;
        while (toTrack.value != targetValue && gameObject != null)
        {
            if (Mathf.Abs(toTrack.value - targetValue) < MINIMUMVALUECHANGE) { toTrack.value = targetValue; }
            else {
                float valueChange = VALUECHANGESPEEDMULTSMALL;
                if (Mathf.Abs(toTrack.value - targetValue) > SPEEDUPMAXVALUEDIFFERENCE) { valueChange = 1f; }
                else if (Mathf.Abs(toTrack.value - targetValue) > SPEEDUPVALUEDIFFERENCE) { valueChange = VALUECHANGESPEEDMULTBIG; }
                toTrack.value += ((toTrack.value - targetValue) > 0f) ? -valueChange * Time.deltaTime : valueChange * Time.deltaTime; }
            yield return null;
            if (toTrack == null) { yield break; }
        }
        lockedCoroutine = false;
    }
}
