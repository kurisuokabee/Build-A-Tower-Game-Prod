
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class BossTimerUI : MonoBehaviour
{
    private TMP_Text timerText;
    private AbyssManager abyssManager;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
        timerText.enabled = false;
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Start()
    {
        // Retry in case AbyssManager was initialized after OnEnable.
        SubscribeToEvents();

        if (abyssManager != null && abyssManager.IsBossEventActive)
            ShowTimer();
        else
            HideTimer(false);
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        if (abyssManager == null)
            abyssManager = AbyssManager.Instance;

        if (abyssManager == null)
            return;

        abyssManager.OnBossTimerChanged -= UpdateTimerDisplay;
        abyssManager.OnBossEventStarted -= ShowTimer;
        abyssManager.OnBossEventEnded -= HideTimer;

        abyssManager.OnBossTimerChanged += UpdateTimerDisplay;
        abyssManager.OnBossEventStarted += ShowTimer;
        abyssManager.OnBossEventEnded += HideTimer;
    }

    private void UnsubscribeFromEvents()
    {
        if (abyssManager == null)
            return;

        abyssManager.OnBossTimerChanged -= UpdateTimerDisplay;
        abyssManager.OnBossEventStarted -= ShowTimer;
        abyssManager.OnBossEventEnded -= HideTimer;
    }

    private void ShowTimer()
    {
        timerText.enabled = true;
    }

    private void HideTimer(bool success)
    {
        timerText.enabled = false;
    }

    private void UpdateTimerDisplay(float timeRemaining, float totalTime)
    {
        timerText.text = $"Boss Timer: {Mathf.Ceil(timeRemaining)}s";
    }
}
