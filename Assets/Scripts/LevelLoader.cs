using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * A class that handles the fail, succeed, pause and "get ready" screens.
 * Notifies other classes when the state changes.
 */
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
    public GameObject hideWhenShowingAny;
    public static LevelLoader instance;
    bool lockFailureSuccess = false;
    bool waitForDown = false;

    // Start is called before the first frame update
    void Awake()
    {
        pausedPlayer = false;
        instance = this;
        PauseBeforePlayerIsReady();
    }

    private void Update()
    {
        if (Input.GetButtonDown(PlayerController.startingInput) && waitForDown) { PlayerIndicatesReady(); }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    /* Set All Screens Off
     *  Arguments: -
     *  
     *  Turns off all the menu screens.
     */
    void SetAllScreensOff()
    {
        showWhenReadying.SetActive(false);
        showWhenRetrying.SetActive(false);
        showWhenPausing.SetActive(false);
        showWhenFinishing.SetActive(false);
        hideWhenShowingAny.SetActive(true);
    }

    /* Pause Before Player Is Ready
     *  Arguments: -
     *  
     *  Set Player Movement Off before the player indicated ready.
     */
    void PauseBeforePlayerIsReady()
    {
        SetAllScreensOff();
        showWhenReadying.SetActive(true);
        hideWhenShowingAny.SetActive(false);
        waitForDown = true;
        Time.timeScale = 0f;
    }

    /* Player Indicates Ready
     *  Arguments: -
     *  
     *  Player wants to start the level.
     */
    public void PlayerIndicatesReady()
    {
        waitForDown = false;
        SetAllScreensOff();
        LevelCanStart?.Invoke();
        Time.timeScale = 1f;
    }

    /* Player Has Failed
     *  Arguments: -
     *  
     *  Sets the player failed state.
     */
    public static void PlayerHasFailed()
    {
        if (instance.lockFailureSuccess) { return; } else { instance.lockFailureSuccess = true; }
        instance.SetAllScreensOff();
        instance.showWhenRetrying.SetActive(true);
        instance.hideWhenShowingAny.SetActive(false);
        Time.timeScale = 0.05f;
    }

    /* Player Indicates Pause Change
     *  Arguments: state, pause on or off.
     *  
     *  Sets the player failed state.
     */
    public static void PlayerIndicatesPauseChange(bool state)
    {
        instance.showWhenPausing.SetActive(state);
        if (state) { Time.timeScale = 0f; instance.hideWhenShowingAny.SetActive(false); }
        else { Time.timeScale = 1f; instance.hideWhenShowingAny.SetActive(true); }
    }

    /* Player Has Succeeded
     *  Arguments: -
     *  
     *  Sets the player success state.
     */
    public static void PlayerHasSucceeded()
    {
        if (instance.lockFailureSuccess) { return; } else { instance.lockFailureSuccess = true; }
        instance.SetAllScreensOff();
        instance.showWhenFinishing.SetActive(true);
        LevelEnds?.Invoke();
    }
}
