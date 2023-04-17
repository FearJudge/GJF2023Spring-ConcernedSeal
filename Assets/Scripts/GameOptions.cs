using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/* 
 * A class that handles player made settings largely.
 */
public class GameOptions : MonoBehaviour
{
    public AudioMixer mixChannelSound;
    public Slider sfxSlider;
    public Slider volSlider;
    public TMPro.TextMeshProUGUI sfxPerc;
    public TMPro.TextMeshProUGUI volPerc;
    public TMPro.TMP_Dropdown resolutionMenu;

    public void OnEnable()
    {
        SetSliders();
    }

    public void SetSFXSound(float value)
    {
        mixChannelSound.SetFloat("SFX", value);
        sfxPerc.text = ((80f + value) * 1.25f).ToString("000") + "%";
    }

    public void SetBGMSound(float value)
    {
        mixChannelSound.SetFloat("BGM", value);
        volPerc.text = ((80f + value) * 1.25f).ToString("000") + "%";
    }

    public void PlaySample()
    {
        SoundManager.PlaySound("Splash", 1f);
    }

    public void SetSliders()
    {
        mixChannelSound.GetFloat("SFX", out float val);
        sfxSlider.value = val;
        mixChannelSound.GetFloat("BGM", out float valbg);
        volSlider.value = valbg;
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    public void GetScreenResolutions()
    {
        List<string> resolutionStrings = new List<string>();
        resolutionMenu.ClearOptions();
        for (int a = 0; a < Screen.resolutions.Length; a++)
        {
            resolutionStrings.Add(Screen.resolutions[a].ToString());
        }
        resolutionMenu.AddOptions(resolutionStrings);
    }

    public void ChangeResolution(int index)
    {
        Resolution r = Screen.resolutions[index];
        Screen.SetResolution(r.width, r.height, Screen.fullScreen, r.refreshRate);
    }
}
