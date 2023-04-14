using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerScript : MonoBehaviour
{
    public AnimatedCounterScript acs;
    public int gameTime = 0;
    float gameTimeSpentBeforeStart = 0f;

    public float frequency = 0.05f;

    // Start is called before the first frame update
    void Start()
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
        acs.Value = Mathf.RoundToInt((Time.timeSinceLevelLoad - gameTimeSpentBeforeStart) * 1000f);
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
