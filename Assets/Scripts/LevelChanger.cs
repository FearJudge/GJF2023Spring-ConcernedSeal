using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Modified from an older project
// https://fearjudge.itch.io/chaos-conjecture
public class LevelChanger : MonoBehaviour
{
    [SerializeField] private Animator myAnim;
    public float finalDur = 2f;

    void Start()
    {
        DontDestroyOnLoad(this);
    }

    public void StartTransition(string toScene, float extraWait = 0f)
    {
        StartCoroutine(AsyncSceneChange(toScene, extraWait));
    }

    public IEnumerator AsyncSceneChange(string to, float wait)
    {
        yield return new WaitForSecondsRealtime(0.4f + wait);
        AsyncOperation sceneChange = SceneManager.LoadSceneAsync(to);
        sceneChange.allowSceneActivation = false;
        yield return new WaitForSecondsRealtime(1.1f);
        while (sceneChange.progress < 0.9f)
        {
            yield return new WaitForSecondsRealtime(0.1f);
        }
        sceneChange.allowSceneActivation = true;
        yield return new WaitUntil(() => sceneChange.isDone);
        yield return new WaitForSecondsRealtime(0.2f);
        EndTransition();
        yield return new WaitForSecondsRealtime(finalDur);
        EndMe();
    }

    public void EndTransition()
    {
        myAnim.SetTrigger("SceneChange");
    }

    private void EndMe()
    {
        Destroy(gameObject);
    }
}