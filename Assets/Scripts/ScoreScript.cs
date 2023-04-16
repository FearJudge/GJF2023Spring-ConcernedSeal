using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreScript : MonoBehaviour
{
    const long BONUSGOLD = 30000;
    const long BONUSPAR = 30000;
    const long BONUSLOSSPERSOVERPAR = 4000;
    const long BONUSNOWATERHIT = 15000;
    const long BONUSNOLIFELOSS = 15000;
    const long BONUSNOSLIDING = 10000;

    static long internalScoreOnStart = 0;
    static long internalScore = 0;
    static AnimatedCounterScript staticCounterInstance;
    public AnimatedCounterScript theCurrentScoreDisplay;
    public static long Score { get { return internalScore; } set { internalScore = value; staticCounterInstance.Value = internalScore; } }

    private void Start()
    {
        internalScoreOnStart = internalScore;
        staticCounterInstance = theCurrentScoreDisplay;
        staticCounterInstance.ChangeValueWithoutAnimations(internalScore, false);
    }

    private void OnDestroy()
    {
        staticCounterInstance = null;
    }

    public static void ResetScore()
    {
        internalScore = 0;
    }

    public static void RefreshScoreToPrevious()
    {
        internalScore = internalScoreOnStart;
    }

    public long BonusFromAllGoldenBubbles()
    {
        if (CollectablePoints.goldenBubbles == 0) { Score += BONUSGOLD; return BONUSGOLD; }
        return 0;
    }

    public long BonusFromNoReset()
    {
        if (LevelManager.tries == 0) { Score += BONUSNOLIFELOSS; return BONUSNOLIFELOSS; }
        return 0;
    }

    public long BonusFromNoWet()
    {
        if (PlayerController.hitsToWater == 0) { Score += BONUSNOWATERHIT; return BONUSNOWATERHIT; }
        return 0;
    }

    public long BonusFromNoSlide()
    {
        if (PlayerController.slidingTime <= 0.05f) { Score += BONUSNOSLIDING; return BONUSNOSLIDING; }
        return 0;
    }

    public long BonusTime()
    {
        float bonus = BONUSPAR + Mathf.Clamp((TimerScript.gameTimeInSeconds - FinishLine.currentLevelPar) * -BONUSLOSSPERSOVERPAR, -BONUSPAR, 0f);
        Score += Mathf.RoundToInt(bonus);
        return Mathf.RoundToInt(bonus);
    }

    public long ScoreThisStage()
    {
        return internalScore - internalScoreOnStart;
    }
}
