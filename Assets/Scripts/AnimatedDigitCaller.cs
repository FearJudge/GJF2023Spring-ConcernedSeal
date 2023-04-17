using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Imported from an older project.
// https://fearjudge.itch.io/chaos-conjecture

/* 
* A class that handles a single digit, used by AnimatedCounterScript.
*/
public class AnimatedDigitCaller : MonoBehaviour
{
    public AnimatedCounterScript parent;
    public int n;

    public void CallChange()
    {
        parent.NonAnimatedChangeToDigit(n);
    }
}
