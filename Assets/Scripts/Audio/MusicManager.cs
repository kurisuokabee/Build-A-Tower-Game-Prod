
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;

    [Header("Menu Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField, Range(0f, 1f)] private float menuVolume = 1f;

    [Header("Gameplay Music")]
    [SerializeField] private List<AudioClip> gameplayTracks = new();
    [SerializeField, Range(0f, 1f)] private float gameplayVolume = 0.7f;

    [Header("Ambience")]
    [SerializeField] private AudioClip ambienceMusic;
    [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.1f;

    private Coroutine playlistCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateMusic(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateMusic(scene.name);
    }

    private void UpdateMusic(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                StopPlaylist();
                StopAmbience();
                PlayMusic(menuMusic, menuVolume);
                break;

            case "Gameplay":
                PlayAmbience(ambienceMusic, ambienceVolume);
                StartPlaylist();
                break;
        }
    }

    private void StartPlaylist()
    {
        if (playlistCoroutine != null)
            return;

        if (gameplayTracks.Count == 0)
            return;

        playlistCoroutine = StartCoroutine(PlayGameplayPlaylist());
    }

    private IEnumerator PlayGameplayPlaylist()
    {
        int index = 0;

        while (true)
        {
            AudioClip clip = gameplayTracks[index];

            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.volume = gameplayVolume;
                musicSource.loop = false;
                musicSource.Play();

                yield return new WaitWhile(() => musicSource.isPlaying);
            }

            index = (index + 1) % gameplayTracks.Count;
        }
    }

    private void StopPlaylist()
    {
        if (playlistCoroutine != null)
        {
            StopCoroutine(playlistCoroutine);
            playlistCoroutine = null;
        }

        if (musicSource != null)
            musicSource.Stop();
    }

    public void PlayMusic(AudioClip clip, float volume)
    {
        if (clip == null || musicSource == null)
            return;

        StopPlaylist();

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = volume;
        musicSource.Play();
    }

    public void PlayAmbience(AudioClip clip, float volume)
    {
        if (clip == null || ambienceSource == null)
            return;

        if (ambienceSource.clip == clip && ambienceSource.isPlaying)
            return;

        ambienceSource.Stop();
        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.volume = volume;
        ambienceSource.Play();
    }

    public void StopMusic()
    {
        StopPlaylist();
    }

    public void StopAmbience()
    {
        if (ambienceSource != null)
            ambienceSource.Stop();
    }
}
