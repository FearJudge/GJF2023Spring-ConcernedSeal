using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public static float progressTowardsGoal = 0f;
    public static float waveTowardsGoal = 0f;

    public float trackInterval = 0.5f;

    private void Awake()
    {
        StartCoroutine(TrackPlayerProgress());
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.gameObject.TryGetComponent<PlayerController>(out PlayerController pc);
        if (!isPlayer) { return; }
        LevelLoader.PlayerHasSucceeded();
    }

    IEnumerator TrackPlayerProgress()
    {
        Transform player = GameObject.Find("PLAYER_SealBody").transform;
        float startX = player.position.x;
        float endX = transform.position.x;
        float totalDistance = Mathf.Abs(endX - startX);

        while (gameObject != null)
        {
            yield return new WaitForSeconds(trackInterval);
            progressTowardsGoal = (totalDistance - Mathf.Abs(endX - player.transform.position.x)) / totalDistance;
        }
    }
}
