using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class GameSettingsController : SingletonGeneric<GameSettingsController>
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private TMP_Text currentBgValue;
    [SerializeField] private TMP_Text currentSfxValue;
    [SerializeField] private Slider bgSlider;
    [SerializeField] private Slider sfxSlider;
    private float maxVol = 20f;
    private float minVol = -30f;

    private void Start()
    {
        audioMixer.GetFloat("BG_Vol", out float bgFloat);
        SetBGVolume(bgFloat);
        bgSlider.value = bgFloat;
        audioMixer.GetFloat("SFX_Vol", out float sfxFloat);
        sfxSlider.value = sfxFloat;
        SetSFXVolume(sfxFloat);
    }

    public void SetBGVolume(float volume)
    {
        if (Mathf.Floor(volume) <= minVol)
        {
            audioMixer.SetFloat("BG_Vol", -80f);
            currentBgValue.text = "0";
            return;
        }
        else if (Mathf.Floor(volume) >= maxVol)
        {
            audioMixer.SetFloat("BG_Vol", 20f);
            currentBgValue.text = "100";
            return;
        }
        audioMixer.SetFloat("BG_Vol", volume);
        currentBgValue.text = Mathf.RoundToInt((volume - minVol) / (maxVol - minVol) * 100).ToString();
    }

    public void SetSFXVolume(float volume)
    {
        if (Mathf.RoundToInt(volume) <= minVol)
        {
            audioMixer.SetFloat("SFX_Vol", -80f);
            currentSfxValue.text = "0";
            return;
        }
        else if (Mathf.Floor(volume) >= maxVol)
        {
            audioMixer.SetFloat("SFX_Vol", 20f);
            currentSfxValue.text = "100";
            return;
        }
        audioMixer.SetFloat("SFX_Vol", volume);
        currentSfxValue.text = Mathf.RoundToInt((volume - minVol) / (maxVol - minVol) * 100).ToString();
    }

}
