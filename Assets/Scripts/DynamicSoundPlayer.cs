using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicSoundPlayer : MonoBehaviour
{
    public string soundToPlay;
    public Transform trackable;
    public CameraTracker.TrackingTarget trackThis;

    public float distanceToStartPlaying = 35f;
    public float distanceToMaxVolume = 5f;
    public bool ignoreX = false;
    public bool ignoreY = false;
    public float updateFrequency = 0.5f;

    AudioSource audioPlayer = null;

    // Start is called before the first frame update
    void Start()
    {
        if (trackable == null) {
            switch (trackThis)
            {
                case CameraTracker.TrackingTarget.Player:
                    trackable = GameObject.Find("PLAYER_SealBody").transform;
                    break;
                case CameraTracker.TrackingTarget.Camera:
                    trackable = Camera.main.transform;
                    break;
                default:
                    break;
            }
        }
        StartCoroutine(UpdateSound());
    }

    // Update is called once per frame
    IEnumerator UpdateSound()
    {
        while (gameObject != null)
        {
            ControlSFX(CalculateDistanceToTarget());
            yield return new WaitForSeconds(updateFrequency);
        }
    }

    private float CalculateDistanceToTarget()
    {
        float distanceX = Mathf.Abs(trackable.position.x - transform.position.x);
        float distanceY = Mathf.Abs(trackable.position.y - transform.position.y);
        float distance = 0f;
        if (ignoreX && ignoreY) { return 0f; }
        if (ignoreX) { distance = distanceY; }
        else if (ignoreY) { distance = distanceX; }
        else { distance = Mathf.Sqrt(Mathf.Pow(distanceX, 2) + Mathf.Pow(distanceY, 2)); }
        return distance;
    }

    void ControlSFX(float distance)
    {
        if (audioPlayer == null)
        {
            if (distance < distanceToStartPlaying) { audioPlayer = SoundManager.PlaySound(soundToPlay, 0f); }
        }
        float vol = 1f - Mathf.Clamp((distance - distanceToMaxVolume) / (distanceToStartPlaying - distanceToMaxVolume), 0f, 1f);
        if (audioPlayer != null)
        {
            audioPlayer.volume = vol;
        }
    }

}
