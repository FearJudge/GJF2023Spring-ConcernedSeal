using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveGlacier : MonoBehaviour
{
    public GameObject wavePrefab;
    [HideInInspector] public WaterWave waterWaveReference;
    public float waveMagnitude = 10f;
    public Transform spawnPosition;
    public Vector2 waveDirection;
    public bool startImmediately = true;
    public float countDownToStart = 0f;
    public bool showToPlayer = true;

    public FragileIcePlatform startingIceberg;

    private void Awake()
    {
        LevelLoader.LevelCanStart += BreakOffChunk;
    }

    private void OnDestroy()
    {
        LevelLoader.LevelCanStart -= BreakOffChunk;
    }

    public void BreakOffChunk()
    {
        StartCoroutine(SequenceForWave());
    }

    IEnumerator SequenceForWave()
    {
        yield return new WaitForSeconds(countDownToStart);
        if (showToPlayer) { CameraTracker.trackableOverride = startingIceberg.transform; LevelLoader.pausedPlayer = true; }
        startingIceberg.BreakMe(waveDirection, waveMagnitude);
        yield return new WaitForSeconds(startingIceberg.TimeUntilSinking());

        GameObject wave = Instantiate(wavePrefab, spawnPosition.position, spawnPosition.rotation);
        waterWaveReference = wave.GetComponent<WaterWave>();
        waterWaveReference.magnitude = waveMagnitude;
        waterWaveReference.movementSpeed = waveDirection;
        yield return new WaitForSeconds(0.3f);
        if (showToPlayer) { CameraTracker.trackableOverride = null; LevelLoader.pausedPlayer = false; }
    }
}
