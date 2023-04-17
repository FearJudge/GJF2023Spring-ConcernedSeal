using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A class that handles level timer.
 * Uses and Animated Counter script for display.
 */
public class TimerScript : MonoBehaviour
{
    public AnimatedCounterScript acs;
    public int gameTime = 0;
    public static float gameTimeInSeconds = 0f;
    float gameTimeSpentBeforeStart = 0f;

    public float frequency = 0.05f;

    // Start is called before the first frame update
    void Awake()
    {
        acs.ChangeValueWithoutAnimations(gameTime, false);
        LevelLoader.LevelCanStart += BeginTimer;
        LevelLoader.LevelEnds += EndTimer;
    }

    private void OnDestroy()
    {
        LevelLoader.LevelCanStart -= BeginTimer;
        LevelLoader.LevelEnds -= EndTimer;
    }

    void IncreaseTime()
    {
        acs.Value = Mathf.Clamp(Mathf.RoundToInt((Time.timeSinceLevelLoad - gameTimeSpentBeforeStart - 2f) * 1000f), 0, 9999999);
        gameTimeInSeconds = acs.Value / 1000f;
    }

    public void BeginTimer()
    {
        gameTimeSpentBeforeStart = Time.timeSinceLevelLoad;
        InvokeRepeating("IncreaseTime", 0f, frequency);
    }

    public void EndTimer()
    {
        IncreaseTime();
        CancelInvoke();
    }
}
