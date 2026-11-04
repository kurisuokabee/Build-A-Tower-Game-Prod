using System.Collections;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] AudioSource[] pool;
    [SerializeField] private SoundLibrary library;
    private readonly System.Collections.Generic.Dictionary<AudioClip, float> nextPlayTime = new();
    public SoundLibrary Library => library;

    int index = 0;

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
        foreach(AudioSource a in pool)
            a.gameObject.SetActive(false);
    }

    public void PlaySound(
        AudioClip clip,
        float volume = 1f,
        float cooldown = 0f)
    {
        if (clip == null)
            return;

        if (nextPlayTime.TryGetValue(clip, out float nextTime)
            && Time.time < nextTime)
            return;

        nextPlayTime[clip] = Time.time + cooldown;

        AudioSource source = GetObject();

        source.loop = false;
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.95f, 1.05f);

        source.Play();

        StartCoroutine(DisableAfterPlayingSound(source));
    }

    public AudioSource PlayLoop(AudioClip clip)
    {   
        
        AudioSource source = GetObject();

        source.clip = clip;
        source.volume = 1;
        source.pitch = Random.Range(0.95f, 1.05f);

        source.loop = true;
        source.Play();

        return source;
    }

    public void StopLoop(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.loop = false;
        source.clip = null;
        source.gameObject.SetActive(false);
    }

    IEnumerator DisableAfterPlayingSound(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);

        source.clip = null;
        source.gameObject.SetActive(false);
    }

    AudioSource GetObject()
    {
        foreach (AudioSource source in pool)
        {
            if (!source.gameObject.activeSelf)
            {
                source.gameObject.SetActive(true);
                return source;
            }
        }

        AudioSource obj = pool[index];
        index = (index + 1) % pool.Length;

        obj.Stop();
        obj.gameObject.SetActive(true);

        return obj;
    }

}
