using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void ChangePlayerProgress(float val)
    {
        targetValue = val;
        if (targetValue == playerProgressionSlider.value) { return; }
        StartCoroutine(LerpToTargetValue(playerProgressionSlider));
    }

    IEnumerator LerpToTargetValue(Slider toTrack)
    {
        if (lockedCoroutine) { yield break; }
        lockedCoroutine = true;
        while (toTrack.value != targetValue)
        {
            if (Mathf.Abs(toTrack.value - targetValue) < MINIMUMVALUECHANGE) { toTrack.value = targetValue; }
            else {
                float valueChange = VALUECHANGESPEEDMULTSMALL;
                if (Mathf.Abs(toTrack.value - targetValue) > SPEEDUPMAXVALUEDIFFERENCE) { valueChange = 1f; }
                else if (Mathf.Abs(toTrack.value - targetValue) > SPEEDUPVALUEDIFFERENCE) { valueChange = VALUECHANGESPEEDMULTBIG; }
                toTrack.value += ((toTrack.value - targetValue) > 0f) ? -valueChange * Time.deltaTime : valueChange * Time.deltaTime; }
            yield return null;
        }
        lockedCoroutine = false;
    }
}