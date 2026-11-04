using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] AudioMixer audioMixer;
    Slider masterSlider;
    Slider musicSlider;
    Slider sfxSlider;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        float master = Mathf.Clamp(PlayerPrefs.GetFloat("MasterVolume", 1f), 0.0001f, 1f);
        float music = Mathf.Clamp(PlayerPrefs.GetFloat("MusicVolume", 1f), 0.0001f, 1f);
        float sfx = Mathf.Clamp(PlayerPrefs.GetFloat("SFXVolume", 1f), 0.0001f, 1f);

        ApplyVolume("MasterVolume", master);
        ApplyVolume("MusicVolume", music);
        ApplyVolume("SFXVolume", sfx);
    }

    public void RegisterSliders(Slider master, Slider music, Slider sfx)
    {
        masterSlider = master;
        musicSlider = music;
        sfxSlider = sfx;

        float masterValue = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxValue = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if(masterSlider)
        {
            masterSlider.onValueChanged.RemoveAllListeners();
            masterSlider.value = masterValue;
            masterSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if(musicSlider)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.value = musicValue;
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if(sfxSlider)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.value = sfxValue;
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }
    
    void ApplyVolume(string parameter, float value)
    {
        // Convert linear 0–1 to decibels (-80 to 0)
        float dB = Mathf.Log10(value) * 20f;
        audioMixer.SetFloat(parameter, dB);
    }

    public void SetMasterVolume(float value)
    {
        ApplyVolume("MasterVolume", value);
        PlayerPrefs.SetFloat("MasterVolume", value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        ApplyVolume("MusicVolume", value);
        PlayerPrefs.SetFloat("MusicVolume", value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        ApplyVolume("SFXVolume", value);
        PlayerPrefs.SetFloat("SFXVolume", value);
        PlayerPrefs.Save();
    }
}
