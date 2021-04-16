using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundChecker : MonoBehaviour
{
    [HideInInspector] public AudioSource sfxPlayer;

    public void InitializeSoundChecker(AudioSource _sfxPlayer)
    {
        sfxPlayer = _sfxPlayer;
    }

    public void PlaySFXSound(AudioClip _sfx, float volume)
    {
        sfxPlayer.clip = _sfx;
        sfxPlayer.loop = false;
        sfxPlayer.volume = volume;

        sfxPlayer.Play();
    }

    public void PlayLoopSFXSound(AudioClip _sfx, float _playTime, float volume)
    {
        sfxPlayer.clip = _sfx;
        sfxPlayer.loop = true;
        sfxPlayer.volume = volume;

        sfxPlayer.Play();

        Invoke("StopSFXSound", _playTime);
    }    

    public void StopSFXSound()
    {
        sfxPlayer.Stop();
    }

    public float Pitch
    {
        get
        {
            return sfxPlayer.pitch;
        }

        set
        {
            sfxPlayer.pitch = value;
        }
    }
    public AudioClip clip
    {
        get
        {
            return sfxPlayer.clip;
        }
    }

    public bool IsPlaying
    {
        get
        {
            return sfxPlayer.isPlaying;
        }
    }

    public float time
    {
        get
        {
            return sfxPlayer.time;
        }
    }

    public float length
    {
        get
        {
            return sfxPlayer.clip.length;
        }
    }

    public bool loop
    {
        get
        {
            return sfxPlayer.loop;
        }
    }

}

public class SoundManager : SingletonClass<SoundManager>
{
    [Range(0.0f, 1.0f)]
    public float masterVolume = 1.0f;
    
    [Range(0.0f, 1.0f)]
    public float BGMVolume = 1.0f;
    
    [Range(0.0f, 1.0f)]
    public float SFXVolume = 1.0f;

    [Header("최대 재생 가능한 효과음 갯수")]
    public int maxSFXSoundCount;

    private int sfxNumber;

    Dictionary<string, AudioClip> soundDic;

    [Space]
    [Header("BGM 사운드 트랙")]
    [SerializeField]AudioSource bgmPlayer;

    [Space]
    [Header("SFX 사운드 트랙 리스트")]
    [SerializeField]SoundChecker[] sfxTracks;

    public AudioMixer mixer;
    public AudioMixerGroup master;
    public AudioMixerGroup bgm;
    public AudioMixerGroup sfx;

    protected override void Awake()
    {
        base.Awake();

        soundDic = new Dictionary<string, AudioClip>();
        GetAudioClip();

        bgmPlayer = gameObject.AddComponent<AudioSource>();
        bgmPlayer.volume = BGMVolume;
        bgmPlayer.outputAudioMixerGroup = bgm;

        sfxTracks = new SoundChecker[maxSFXSoundCount];

        for (int i = 0; i < maxSFXSoundCount; i++)
        {
            string trackname = "SFXTrack";

            if (i < 10) trackname += "0";

            trackname += i;

            GameObject SFXTrack = new GameObject(trackname);

            SFXTrack.transform.SetParent(transform);

            AudioSource sfxPlayer = SFXTrack.AddComponent<AudioSource>();
            sfxPlayer.outputAudioMixerGroup = sfx;

            SoundChecker soundChecker = SFXTrack.AddComponent<SoundChecker>();
            soundChecker.InitializeSoundChecker(sfxPlayer);

            sfxTracks[i] = soundChecker;
        }
    }

    private void Start()
    {
        InitAudioSetting();
    }

    public void InitAudioSetting()
    {
        if (PlayerPrefs.HasKey("BGMVolume")) // 볼륨이 있고
        {
            SetBGMVolume(PlayerPrefs.GetFloat("BGMVolume")); // 사운드값
        }
        else
        {
            SetBGMVolume(1);
        }

        if (PlayerPrefs.HasKey("SFXVolume")) // 볼륨이 있고
        {
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume")); // 사운드값
        }
        else
        {
            SetSFXVolume(1);
        }
    }

    private void GetAudioClip()
    {
        if (soundDic == null)
        {
            soundDic = new Dictionary<string, AudioClip>();
        }
        Object[] audios = Resources.LoadAll("Sounds");

        for (int i = 0; i < audios.Length; i++)
        {
            if (audios[i].GetType() == typeof(AudioClip))
            {
                AudioClip _audio = audios[i] as AudioClip;

                string clipName = CheckClipNameExist(_audio.name, 1);

                soundDic.Add(clipName, _audio);
            }
        }
    }

    private string CheckClipNameExist(string _clipName, int _index)
    {
        string clipName = _clipName;

        if (soundDic.ContainsKey(_clipName))
        {
            clipName = _clipName + "(" + _index + ")";

            clipName = CheckClipNameExist(clipName, _index);
        }

        return clipName;
    }

    public void PlayBGM(string _soundName, bool _loop = true)
    {
        if (!soundDic.ContainsKey(_soundName)) return;

        if (bgmPlayer.clip != null)
        {
            if (bgmPlayer.clip.name == _soundName) return;
        }
        bgmPlayer.clip = soundDic[_soundName];
        bgmPlayer.loop = _loop;
        bgmPlayer.Play();
    }

    public void PlaySFX(string _soundName, float volume = 1.0f, bool playingCheck = false, float playProgress = 0f)
    {
        if (soundDic.ContainsKey(_soundName))
        {
            if (playingCheck)
            {
                for (int i = 0; i < maxSFXSoundCount; i++)
                {
                    if (sfxTracks[i].IsPlaying)
                    {
                        if (sfxTracks[i].clip == soundDic[_soundName])
                        {
                            float progress = sfxTracks[i].time / sfxTracks[i].length;
                            if (progress <= playProgress)
                            {
                                return;
                            }
                        }
                    }
                }
            }


            GetEmptySFXTrack().PlaySFXSound(soundDic[_soundName], volume);
        }
    }

    public void PlaySFXLoop(string _soundName, float _playTime, float volume = 1.0f)
    {
        if (soundDic.ContainsKey(_soundName))
        {
            GetEmptySFXTrack().PlayLoopSFXSound(soundDic[_soundName], _playTime, volume);
        }
    }

    public SoundChecker GetEmptySFXTrack()
    {
        int indexNum = sfxNumber;
        int lageIndex = 0;
        float lageProgress = 0;
        for (int i = 0; i < maxSFXSoundCount; i++)
        {
            indexNum = (indexNum + 1) % maxSFXSoundCount;

            if (!sfxTracks[indexNum].IsPlaying)
            {
                sfxNumber = indexNum;
                return sfxTracks[indexNum];
            }

            float progress = sfxTracks[i].time / sfxTracks[i].length;
            if (progress > lageProgress && !sfxTracks[i].loop)
            {
                lageIndex = i;
                lageProgress = progress;
            }
        }

        return sfxTracks[lageIndex];
    }

    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void PauseBGM()
    {
        bgmPlayer.Pause();
    }

    public void ContinueBGM()
    {
        bgmPlayer.Play();
    }

    public void StopAllSound()
    {
        bgmPlayer.Stop();
        bgmPlayer.clip = null;

        StopAllSFXSound();
    }

    public void StopAllSFXSound()
    {
        for (int i = 0; i < sfxTracks.Length; i++)
        {
            sfxTracks[i].CancelInvoke("StopSFXSound");
            sfxTracks[i].StopSFXSound();
        }
    }

    public void SetPitch(float pitch)
    {
        bgmPlayer.pitch = pitch;

        for (int i = 0; i < sfxTracks.Length; i++)
        {
            sfxTracks[i].Pitch = pitch;
        }
    }

    public void SetBGMPitch(float pitch)
    {
        bgmPlayer.pitch = pitch;
    }

    #region volume
    public void SetVolume(float _masterVolume, float _SFXVolume, float _BGMVolume)
    {
        SetBGMVolume(_SFXVolume, false);
        SetSFXVolume(_BGMVolume, false);
        SetMasterVolume(_masterVolume);
    }

    public void SetMasterVolume(float _volume, bool _calc = true)
    {
        masterVolume = _volume;

        if (!_calc) return;

        bgmPlayer.volume = masterVolume * BGMVolume;

        for (int i = 0; i < sfxTracks.Length; i++)
        {
            sfxTracks[i].sfxPlayer.volume = _volume;
        }
    }

    public void SetSFXVolume(float _volume, bool _calc = true)
    {
        SFXVolume = _volume;

        if (!_calc) return;

        if (SFXVolume <= 0.01)
        {
            mixer.SetFloat("SFX", -80);
        }
        else
        {
            mixer.SetFloat("SFX", SFXVolume * 20 - 20);
        }
    }

    public void SetBGMVolume(float _volume, bool _calc = true)
    {
        BGMVolume = _volume;

        if (!_calc) return;

        if (BGMVolume <= 0.01)
        {
            mixer.SetFloat("BGM", -80);
        }
        else
        {
            mixer.SetFloat("BGM", BGMVolume * 20 - 20);
        }
    }
#endregion volume
}
