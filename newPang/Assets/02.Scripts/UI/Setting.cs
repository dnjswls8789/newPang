using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Setting : MonoBehaviour
{
    [SerializeField] Slider bgm;
    [SerializeField] Slider sfx;

    float bgmVolume = 1f;
    float sfxVolume = 1f;
    string bgmKey = "BGMVolume";
    string sfxKey = "SFXVolume";

    private void Start()
    {
        //InitSetting();
        //
        //gameObject.SetActive(false);
    }

    public void InitSetting()
    {
        bgm.onValueChanged.AddListener(delegate { BgmValueCangeCheck(); });
        sfx.onValueChanged.AddListener(delegate { SfxValueCangeCheck(); });

        if (PlayerPrefs.HasKey(bgmKey))
        {
            bgmVolume = PlayerPrefs.GetFloat(bgmKey);
        }

        if (PlayerPrefs.HasKey(sfxKey))
        {
            sfxVolume = PlayerPrefs.GetFloat(sfxKey);
        }

        PlayerPrefs.SetFloat(bgmKey, bgmVolume);
        PlayerPrefs.SetFloat(sfxKey, sfxVolume);

        bgm.value = bgmVolume;
        sfx.value = sfxVolume;

    }

    public void BgmValueCangeCheck()
    {
        bgmVolume = bgm.value;
        PlayerPrefs.SetFloat(bgmKey, bgmVolume);
        SoundManager.GetInstance.SetBGMVolume(bgmVolume);
    }

    public void SfxValueCangeCheck()
    {
        sfxVolume = sfx.value;
        PlayerPrefs.SetFloat(sfxKey, sfxVolume);
        SoundManager.GetInstance.SetSFXVolume(sfxVolume);
    }
}
