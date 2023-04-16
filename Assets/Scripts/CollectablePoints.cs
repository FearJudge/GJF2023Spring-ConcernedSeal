using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectablePoints : MonoBehaviour
{
    public static int goldenBubbles = 0;
    const int THRESHOLDBIG = 100;
    public int scoreUp = 15;
    public bool isGolden = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            ScoreScript.Score += scoreUp;
            if (scoreUp > THRESHOLDBIG) { SoundManager.PlaySound("BubbleBig"); }
            else { SoundManager.PlaySound("BubbleSmall"); }
            Destroy(this.gameObject);
        }
    }

    private void Awake()
    {
        if (isGolden) { goldenBubbles++; }
    }

    private void OnDestroy()
    {
        if (isGolden) { goldenBubbles--; }
    }
}
