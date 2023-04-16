using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelFinishUI : MonoBehaviour
{
    const float WAIT = 0.7f;
    const float WAITFINAL = 1f;
    public TextMeshProUGUI[] bonuses;
    public TextMeshProUGUI totalThisStage;
    public ScoreScript score;

    // Start is called before the first frame update
    void Awake()
    {
        LevelLoader.LevelEnds += BeginFinish;
    }

    private void OnDestroy()
    {
        LevelLoader.LevelEnds -= BeginFinish;
    }

    void BeginFinish()
    {
        StartCoroutine(GetBonuses());
    }

    IEnumerator GetBonuses()
    {
        ulong total = (ulong)score.ScoreThisStage();
        yield return new WaitForSeconds(WAIT);
        long bonus0 = score.BonusTime();
        bonuses[0].text = bonus0.ToString();
        yield return new WaitForSeconds(WAIT);
        long bonus1 = score.BonusFromNoReset();
        bonuses[1].text = bonus1.ToString();
        yield return new WaitForSeconds(WAIT);
        long bonus2 = score.BonusFromAllGoldenBubbles();
        bonuses[2].text = bonus2.ToString();
        yield return new WaitForSeconds(WAIT);
        long bonus3 = score.BonusFromNoSlide();
        bonuses[3].text = bonus3.ToString();
        yield return new WaitForSeconds(WAIT);
        long bonus4 = score.BonusFromNoWet();
        bonuses[4].text = bonus4.ToString();
        yield return new WaitForSeconds(WAITFINAL);
        totalThisStage.text = total.ToString();
        yield return new WaitForSeconds(0.4f);
        total += (ulong)(bonus0 + bonus1 + bonus2 + bonus3 + bonus4);
        totalThisStage.text = total.ToString();
    }
}
