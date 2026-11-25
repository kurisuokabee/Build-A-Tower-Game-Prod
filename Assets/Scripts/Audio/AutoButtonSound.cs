using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.EventSystems;

public class AutoButtonSound : MonoBehaviour
{
    public static AutoButtonSound Instance;

    public AudioClip clickSound;
    public AudioClip hoverSound;

    void Awake()
    {
        // Singleton (only one allowed)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Listen to scene changes
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ApplyToAllButtons();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToAllButtons();
    }

   
    void ApplyToAllButtons()
    {
        Button[] buttons = FindObjectsByType<Button>();

        foreach (Button btn in buttons)
        {
            // Click
            btn.onClick.RemoveListener(PlayClick);
            btn.onClick.AddListener(PlayClick);

            // Hover
            EventTrigger trigger = btn.GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            // Remove previous hover event (prevents duplicates)
            trigger.triggers.RemoveAll(x => x.eventID == EventTriggerType.PointerEnter);

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((_) => PlayHover());

            trigger.triggers.Add(entry);
        }
    }

    void PlayHover()
    {
        SoundFXManager.Instance.PlaySound(hoverSound);
    }

    void PlayClick()
    {
        SoundFXManager.Instance.PlaySound(clickSound);
    }
}