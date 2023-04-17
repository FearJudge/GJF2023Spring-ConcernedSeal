using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Imported from an older project.
// https://fearjudge.itch.io/chaos-conjecture

/* 
 * A class that handles animated digit counters.
 * Takes in a value (long Value), and sets numbers it controls to
 * match the counters Value by increasing or decreasing them.
 */
public class AnimatedCounterScript : MonoBehaviour
{
    long displayval = 0;
    long val = 0;
    // bool soundPlayed = false; // Used in a now removed tick sound in the previous project.
    public string normalIncrementTrigger = "incNormal";
    public string normalDecrementTrigger = "decNormal";
    public string rolloverIncrementTrigger = "incRollover";
    public string rolloverDecrementTrigger = "decRollover";
    public long Value
    {
        set
        {
            val = value;
            if (val < 0) { val = 0; }
            else if (val.ToString().Length > digits.Length) { val = long.Parse(new string('9', digits.Length)); }
            CheckMode();
        }
        get
        {
            return val;
        }
    }
    [System.Serializable]
    public class AnimatedDigit
    {
        public TMPro.TextMeshProUGUI digit;
        public int value = 0;
        public int valueToChange = 0;
        public Animator animator;
    }
    [SerializeField] AnimatedDigit[] digits;
    public bool rollingCounter = true;
    public AudioClip onTick;
    public float onTickVol = 0.8f;
    public float rollingCounterSpeed = 0.14f;
    public int[] changeOrder = new int[3] {100, 10, 1};
    bool animating = false;

    private void Awake()
    {
        for (int a = 0; a < digits.Length; a++)
        {
            digits[a].animator = digits[a].digit.gameObject.GetComponent<Animator>();
        }
        CheckMode();
    }

    private void OnEnable()
    {
        if (animating) { animating = false; }
        CheckMode();
    }

    void CheckMode()
    {
        if (animating) { return; }
        StartCoroutine(BeginAnimating());
    }

    // Use this to Set up the Value without going through the animation process.
    // Useful for resetting a counter value.
    // PARAMETERS:
    // value - Value to change.
    // sound - Wether to play the tick sound on change or not.
    public void ChangeValueWithoutAnimations(long value, bool sound = true)
    {
        val = value;
        if (val < 0) { val = 0; }
        else if (val.ToString().Length > digits.Length) { val = long.Parse(new string('9', digits.Length)); }
        displayval = val;
        int b = 0;
        for (int a = digits.Length - 1; a >= 0; a--)
        {
            long n = displayval / (long)Mathf.Pow(10, b) % 10;
            digits[a].value = (int)n;
            digits[a].valueToChange = (int)n;
            NonAnimatedChangeToDigit(a, sound);
            b++;
        }
    }

    // Use this to Set up the Displayed Value without affecting the underlying true value.
    // Useful for things like Countdowns.
    // PARAMETERS:
    // value - Value to change.
    // sound - Wether to play the tick sound on change or not.
    public void ChangeDisplayWithoutAnimations(long value, bool sound = true)
    {
        displayval = value;
        int b = 0;
        for (int a = digits.Length - 1; a >= 0; a--)
        {
            long n = displayval / (long)Mathf.Pow(10, b) % 10;
            digits[a].valueToChange = (int)n;
            NonAnimatedChangeToDigit(a, sound);
            b++;
        }
    }

    // Use this to set the underlying value to whatever is displayed.
    // Useful for things like rolling counters that can be interrupted.
    public void SetValueToCurrent()
    {
        val = displayval;
    }

    // Change a digit without animating. Used internally.
    public void NonAnimatedChangeToDigit(int digit, bool sound = true)
    {
        digits[digit].value = digits[digit].valueToChange;
        digits[digit].digit.text = digits[digit].value.ToString();
    }

    void DisplayUp()
    {
        for (int c = 0; c < changeOrder.Length; c++)
        {
            if (displayval + changeOrder[c] <= val) { displayval += changeOrder[c]; c = changeOrder.Length; }
        }
        int b = 0;
        for (int a = digits.Length - 1; a >= 0; a--)
        {
            long n = displayval / (long)Mathf.Pow(10, b) % 10;
            digits[a].valueToChange = (int)n;
            if (n > digits[a].value)
            {
                digits[a].animator.SetTrigger(normalIncrementTrigger);
            }
            if (n < digits[a].value)
            {
                digits[a].animator.SetTrigger(rolloverIncrementTrigger);
            }
            b++;
        }
    }

    void DisplayDown()
    {
        for (int c = 0; c < changeOrder.Length; c++)
        {
            if (displayval - changeOrder[c] >= val) { displayval -= changeOrder[c]; c = changeOrder.Length; }
        }
        int b = 0;
        for (int a = digits.Length - 1; a >= 0; a--)
        {
            long n = displayval / (long)Mathf.Pow(10, b) % 10;
            digits[a].valueToChange = (int)n;
            if (n < digits[a].value)
            {
                digits[a].animator.SetTrigger(normalDecrementTrigger);
            }
            if (n > digits[a].value)
            {
                digits[a].animator.SetTrigger(rolloverDecrementTrigger);
            }
            b++;
        }
    }

    IEnumerator BeginAnimating()
    {
        animating = true;
        while (displayval != val)
        {
            if (displayval < val)
            {
                DisplayUp();
            }
            if (displayval > val)
            {
                DisplayDown();
            }
            yield return new WaitForSecondsRealtime(rollingCounterSpeed);
            // soundPlayed = false;
        }
        animating = false;
    }
}
