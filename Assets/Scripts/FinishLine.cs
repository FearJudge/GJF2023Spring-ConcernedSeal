using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public delegate void ProgressChangedNotifier(float value);
    public static ProgressChangedNotifier PlayerProgressChanged;
    public static ProgressChangedNotifier WaveProgressChanged;
    public delegate void FinishPositionNotifier(Transform item);
    public static FinishPositionNotifier FinishedPositionRing;

    public static float progressTowardsGoal = 0f;
    public static float waveTowardsGoal = 0f;
    public static float currentLevelPar = 0f;

    public float levelPar = 25f;

    public float trackInterval = 0.5f;
    public Transform ring;
    bool finished = false;
    public ParticleSystem particleSplash;

    private void Start()
    {
        currentLevelPar = levelPar;
        StartCoroutine(TrackPlayerProgress());
        if (ring == null) { transform.GetChild(0); }
    }

    private void OnDestroy()
    {
        PlayerController.HaveMovedToGoal -= Splash;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.gameObject.TryGetComponent<PlayerController>(out PlayerController pc);
        if (!isPlayer) { return; }
        LevelLoader.PlayerHasSucceeded();
        LevelLoader.pausedPlayer = true;
        FinishedPositionRing?.Invoke(ring);
        finished = true;
        PlayerController.HaveMovedToGoal += Splash;
    }

    IEnumerator TrackPlayerProgress()
    {
        Transform player = GameObject.Find("PLAYER_SealBody").transform;
        float startX = player.position.x;
        float endX = transform.position.x;
        float totalDistance = Mathf.Abs(endX - startX);

        while (gameObject != null && !finished)
        {
            yield return new WaitForSeconds(trackInterval);
            progressTowardsGoal = (totalDistance - Mathf.Abs(endX - player.transform.position.x)) / totalDistance;
            PlayerProgressChanged?.Invoke(progressTowardsGoal);
        }
    }

    void Splash()
    {
        particleSplash.Play();
        SoundManager.PlaySound("Splash", 0.9f);
    }
}
