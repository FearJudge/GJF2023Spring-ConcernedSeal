using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    const string mainmenu = "MainMenu";

    [System.Serializable]
    public struct MenuScreen
    {
        public GameObject root;
        public Button[] buttons;
        public int depth;
        public int variant;
    }

    public MenuScreen[] screens;
    public int currentDepth = 0;
    public GameObject lc;

    public void GoBackScreen(int toVariant=0)
    {
        currentDepth--;
        SetActiveScreen(currentDepth, toVariant);
    }

    public void GoForwardScreen(int variant=0)
    {
        currentDepth++;
        SetActiveScreen(currentDepth, variant);
    }

    public void SetActiveScreen(int thisDepth, int variant)
    {
        for (int a = 0; a < screens.Length; a++)
        {
            if (screens[a].depth < thisDepth)
            {
                for (int b = 0; b < screens[a].buttons.Length; b++)
                {
                    screens[a].buttons[b].interactable = false;
                }
            }
            else if (screens[a].depth == thisDepth && screens[a].variant == variant)
            {
                screens[a].root.gameObject.SetActive(true);
                for (int b = 0; b < screens[a].buttons.Length; b++)
                {
                    screens[a].buttons[b].interactable = true;
                }
            }
            else
            {
                screens[a].root.gameObject.SetActive(false);
            }
        }
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    public void GoToScene(string name)
    {
        GameObject changer = Instantiate(lc);
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
        transitionscript.StartTransition(name, 0.1f);
    }

    public void GoToGame()
    {
        GameObject changer = Instantiate(lc);
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
    }

    public void ResetScene()
    {
        GameObject changer = Instantiate(lc);
        string current = SceneManager.GetActiveScene().name;
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
        transitionscript.StartTransition(current, 0.1f);
    }

    public void NextScene()
    {
        void NextStandard()
        {
            string next = LevelManager.GetNextStandardLevelName();
            Debug.Log(SceneUtility.GetBuildIndexByScenePath(next));
            if (SceneUtility.GetBuildIndexByScenePath(next) == -1) { ReturnToMainMenu(); return; }
            GameObject changer = Instantiate(lc);
            LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
            transitionscript.StartTransition(next, 0.1f);
        }

        void NextCustom()
        {
            if (!LevelManager.NextCustomLevelExists()) { ReturnToMainMenu(); return; }
            GameObject changer = Instantiate(lc);
            LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
            transitionscript.StartTransition(LevelManager.CUSTOMLEVELSCENE, 0.1f);
        }

        if (!LevelManager.isGoingThroughCustomData) { NextStandard(); }
        else { NextCustom(); }
    }

    public void ReturnToMainMenu()
    {
        GameObject changer = Instantiate(lc);
        LevelChanger transitionscript = changer.GetComponent<LevelChanger>();
        transitionscript.StartTransition(mainmenu, 0.1f);
    }
}