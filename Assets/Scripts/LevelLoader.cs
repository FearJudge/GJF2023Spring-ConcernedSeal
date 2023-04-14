using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    public delegate void LevelStateChanged();
    public static event LevelStateChanged LevelCanStart;
    public static event LevelStateChanged LevelEnds;

    public static bool pausedPlayer = false;
    public bool isPreloaded = true;
    public GameObject showWhenReadying;
    public GameObject showWhenRetrying;
    public GameObject showWhenPausing;
    public GameObject showWhenFinishing;
    public static LevelLoader instance;
    bool lockFailureSuccess = false;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        PauseBeforePlayerIsReady();
    }

    private void OnDestroy()
    {
        instance = null;
    }

    void SetAllScreensOff()
    {
        showWhenReadying.SetActive(false);
        showWhenRetrying.SetActive(false);
        showWhenPausing.SetActive(false);
        showWhenFinishing.SetActive(false);
    }

    void PauseBeforePlayerIsReady()
    {
        SetAllScreensOff();
        showWhenReadying.SetActive(true);
        Time.timeScale = 0f;
    }

    public void PlayerIndicatesReady()
    {
        SetAllScreensOff();
        LevelCanStart?.Invoke();
        Time.timeScale = 1f;
    }

    public static void PlayerHasFailed()
    {
        if (instance.lockFailureSuccess) { return; } else { instance.lockFailureSuccess = true; }
        instance.SetAllScreensOff();
        instance.showWhenRetrying.SetActive(true);
        Time.timeScale = 0.05f;
    }

    public static void PlayerIndicatesPauseChange(bool state)
    {
        instance.showWhenPausing.SetActive(state);
        if (state) { Time.timeScale = 0f; }
        else { Time.timeScale = 1f; }
    }

    public static void PlayerHasSucceeded()
    {
        if (instance.lockFailureSuccess) { return; } else { instance.lockFailureSuccess = true; }
        instance.SetAllScreensOff();
        instance.showWhenFinishing.SetActive(true);
        LevelEnds?.Invoke();
    }
}
