using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayerAndLibrary : MonoBehaviour
{
    public AudioMixer mix;
    public AudioMixerGroup sfxMix;
    public List<SoundManager.AudioGroup> allSounds = new List<SoundManager.AudioGroup>();
    public static SoundPlayerAndLibrary instance;
    public static AudioSource TwoDSound;
    public static List<GameObject> currentSounds = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        TwoDSound = GetComponent<AudioSource>();
        SoundManager.mixSfx = sfxMix;

        if (allSounds.Count <= SoundManager.allSounds.Count) { return; }
        for (int a = 0; a < allSounds.Count; a++)
        {
            if (SoundManager.allSounds.Exists(p => p.soundType.ToLower() == allSounds[a].soundType.ToLower())) { continue; }
            SoundManager.allSounds.Add(allSounds[a]);
        }
    }

    private void Start()
    {
        SubscribeToPlayerEvents(true);
    }

    private void OnDestroy()
    {
        SubscribeToPlayerEvents(false);
        instance = null;
        TwoDSound = null;
    }

    void SubscribeToPlayerEvents(bool subscribe)
    {
        if (subscribe) { PlayerController.HaveLandedOnSnow += PlaySnowSFX; } else { PlayerController.HaveLandedOnSnow -= PlaySnowSFX; }
        if (subscribe) { PlayerController.HaveHitWater += PlayWaterSFX; } else { PlayerController.HaveLandedOnSnow -= PlayWaterSFX; }
        if (subscribe) { PlayerController.HaveBegunSliding += PlayIceDingSFX; } else { PlayerController.HaveLandedOnSnow -= PlayIceDingSFX; }
        if (subscribe) { PlayerController.HaveJumped += PlayJumpSFX; } else { PlayerController.HaveLandedOnSnow -= PlayJumpSFX; }
        if (subscribe) { PlayerController.HaveFinishedLevel += PlayFinishSFX; } else { PlayerController.HaveFinishedLevel -= PlayFinishSFX; }
    }

    void PlaySnowSFX()
    {
        SoundManager.PlaySound("SnowHit");
    }

    void PlayWaterSFX()
    {
        SoundManager.PlaySound("Splash");
    }

    void PlayIceDingSFX()
    {
        SoundManager.PlaySound("SlideSnap");
    }

    void PlayJumpSFX()
    {
        SoundManager.PlaySound("Jump");
    }

    void PlayFinishSFX()
    {
        SoundManager.PlaySound("Finish");
    }
}

public static class SoundManager
{
    [System.Serializable]
    public struct AudioGroup
    {
        public string soundType;
        public AudioClip[] sounds;
        public bool isLooping;
    }
    public static List<AudioGroup> allSounds = new List<AudioGroup>();
    public static AudioMixerGroup mixSfx;

    static AudioClip GetClipFromGroup(string soundType, out bool isLooping)
    {
        isLooping = false;
        AudioGroup group = allSounds.Find(p => p.soundType.ToLower() == soundType.ToLower());
        if (group.sounds.Length == 0) { return allSounds[0].sounds[0]; }
        isLooping = group.isLooping;
        return group.sounds[Random.Range(0, group.sounds.Length)];
    }

    public static AudioSource PlaySound(string soundType, Vector3 pos, float volMult = 0.75f)
    {
        mixSfx.audioMixer.GetFloat("SFX", out float volMix);
        volMix = ((80f - volMix) / 80f);
        return PlaySFX(GetClipFromGroup(soundType, out bool loop), pos, volMult * volMix, 1f);
    }

    public static AudioSource PlaySound(string soundType, float volMult = 0.75f)
    {
        return PlaySFX(GetClipFromGroup(soundType, out bool loop), volMult, 1f, loop);
    }

    public static AudioSource PlaySFX(AudioClip clip, float volume, float pitch, bool loop = false)
    {
        GameObject audio = new GameObject("SoundEffect", typeof(AudioSource));
        var source = audio.GetComponent<AudioSource>();
        SoundPlayerAndLibrary.currentSounds.Add(audio);
        source.loop = loop;
        source.pitch = pitch;
        source.volume = volume;
        source.outputAudioMixerGroup = mixSfx;
        source.clip = clip;
        source.Play();
        SoundPlayerAndLibrary.instance.StartCoroutine(PlayMe(source, audio));
        return source;
    }

    public static AudioSource PlaySFX(AudioClip clip, Vector3 pos, float volume, float pitch, bool loop = false)
    {
        GameObject audio = new GameObject("SoundEffect", typeof(AudioSource));
        audio.transform.position = pos;
        var source = audio.GetComponent<AudioSource>();
        SoundPlayerAndLibrary.currentSounds.Add(audio);
        source.spatialBlend = 1f;
        source.spatialize = true;
        source.loop = loop;
        source.pitch = pitch;
        source.volume = volume;
        source.outputAudioMixerGroup = mixSfx;
        source.clip = clip;
        source.Play();
        SoundPlayerAndLibrary.instance.StartCoroutine(PlayMe(source, audio));
        return source;
    }

    static IEnumerator PlayMe(AudioSource src, GameObject obj)
    {
        while (src.isPlaying)
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }
        SoundPlayerAndLibrary.currentSounds.Remove(obj);
        Object.Destroy(obj);
        yield return null;
    }
}
