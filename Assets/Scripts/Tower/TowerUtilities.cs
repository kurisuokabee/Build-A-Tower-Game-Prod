
using UnityEngine;
using UnityEngine.UI;

public class TowerUtilities : MonoBehaviour
{
    public static TowerUtilities Instance { get; private set; }

    [Header("Utility Buttons")]
    [SerializeField] private Button rageButton;
    [SerializeField] private Button zealButton;
    [SerializeField] private Button serenityButton;

    [Header("Rage")]
    [SerializeField] private float rageDuration = 10f;
    [SerializeField] private float rageCooldown = 180f;
    [SerializeField] private float rageMultiplier = 1.5f;

    [Header("Zeal")]
    [SerializeField] private float zealDuration = 5f;
    [SerializeField] private float zealCooldown = 240f;

    [Header("Serenity")]
    [SerializeField] private float serenityDuration = 3f;
    [SerializeField] private float serenityCooldown = 300f;

    private float rageEndTime;
    private float zealEndTime;
    private float serenityEndTime;

    private float rageReadyTime;
    private float zealReadyTime;
    private float serenityReadyTime;

    public bool IsRageActive => Time.time < rageEndTime;
    public bool IsZealActive => Time.time < zealEndTime;
    public bool IsSerenityActive => Time.time < serenityEndTime;

    public float RageMultiplier => rageMultiplier;

    public float RageCooldownRemaining =>
        Mathf.Max(0f, rageReadyTime - Time.time);

    public float ZealCooldownRemaining =>
        Mathf.Max(0f, zealReadyTime - Time.time);

    public float SerenityCooldownRemaining =>
        Mathf.Max(0f, serenityReadyTime - Time.time);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rageButton != null)
            rageButton.onClick.AddListener(UseRage);

        if (zealButton != null)
            zealButton.onClick.AddListener(UseZeal);

        if (serenityButton != null)
            serenityButton.onClick.AddListener(UseSerenity);
    }

    private void Start()
    {
        UpdateButtonStates();
    }

    private void Update()
    {
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (rageButton != null)
            rageButton.interactable = RageCooldownRemaining <= 0f;

        if (zealButton != null)
            zealButton.interactable = ZealCooldownRemaining <= 0f;

        if (serenityButton != null)
            serenityButton.interactable = SerenityCooldownRemaining <= 0f;
    }

    private void OnDestroy()
    {
        if (rageButton != null)
            rageButton.onClick.RemoveListener(UseRage);

        if (zealButton != null)
            zealButton.onClick.RemoveListener(UseZeal);

        if (serenityButton != null)
            serenityButton.onClick.RemoveListener(UseSerenity);

        if (Instance == this)
            Instance = null;
    }

    public void UseRage()
    {
        if (RageCooldownRemaining > 0f)
            return;

        rageEndTime = Time.time + rageDuration;
        rageReadyTime = Time.time + rageCooldown;

        Debug.Log($"Rage activated! {rageMultiplier}x attack speed and damage for {rageDuration} seconds.");

        UpdateButtonStates();
    }

    public void UseZeal()
    {
        if (ZealCooldownRemaining > 0f)
            return;

        zealEndTime = Time.time + zealDuration;
        zealReadyTime = Time.time + zealCooldown;

        Debug.Log($"Zeal activated! Blocks and EXP rewards are doubled for {zealDuration} seconds.");

        UpdateButtonStates();
    }

    public void UseSerenity()
    {
        if (SerenityCooldownRemaining > 0f)
            return;

        serenityEndTime = Time.time + serenityDuration;
        serenityReadyTime = Time.time + serenityCooldown;

        Debug.Log($"Serenity activated! Sakero attacks all targets for {serenityDuration} seconds.");

        UpdateButtonStates();
    }
}
