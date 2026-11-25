
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TowerUtilities : MonoBehaviour
{
    public static TowerUtilities Instance { get; private set; }

    [Header("Utility Buttons")]
    [SerializeField] private Button rageButton;
    [SerializeField] private Button zealButton;
    [SerializeField] private Button serenityButton;

    [Header("Cooldown Sliders")]
    [SerializeField] private Slider rageCooldownSlider;
    [SerializeField] private Slider zealCooldownSlider;
    [SerializeField] private Slider serenityCooldownSlider;

    [Header("Slider Backgrounds")]
    [SerializeField] private Image rageBackground;
    [SerializeField] private Image zealBackground;
    [SerializeField] private Image serenityBackground;

    [SerializeField] private Color cooldownColor = Color.gray;
    [SerializeField] private Color readyColor = Color.white;

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

    [Header("Active Utility Effect")]
    [SerializeField] private ParticleSystem activeUtilityEffect;
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

        InitializeSlider(rageCooldownSlider);
        InitializeSlider(zealCooldownSlider);
        InitializeSlider(serenityCooldownSlider);
    }

    private void Start()
    {
        // Start all utilities on cooldown.
        rageReadyTime = Time.time + rageCooldown;
        zealReadyTime = Time.time + zealCooldown;
        serenityReadyTime = Time.time + serenityCooldown;

        UpdateButtonStates();
        UpdateCooldownSliders();
    }

    private void Update()
    {   
        HandleUtilityInput();
        UpdateButtonStates();
        UpdateCooldownSliders();
        UpdateUtilityEffect();
    }

    private void InitializeSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0f;
    }
    private void UpdateUtilityEffect()
    {
        if (activeUtilityEffect == null)
            return;

        bool anyUtilityActive =
            IsRageActive ||
            IsZealActive ||
            IsSerenityActive;

        if (anyUtilityActive && !activeUtilityEffect.isPlaying)
        {
            activeUtilityEffect.Play();
        }
        else if (!anyUtilityActive && activeUtilityEffect.isPlaying)
        {
            activeUtilityEffect.Stop();
        }
    }

    private void HandleUtilityInput()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            UseRage();

        if (Keyboard.current.zKey.wasPressedThisFrame)
            UseZeal();

        if (Keyboard.current.sKey.wasPressedThisFrame)
            UseSerenity();
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

    private void UpdateCooldownSliders()
    {
        UpdateSlider(
            rageCooldownSlider,
            rageBackground,
            RageCooldownRemaining,
            rageCooldown
        );

        UpdateSlider(
            zealCooldownSlider,
            zealBackground,
            ZealCooldownRemaining,
            zealCooldown
        );

        UpdateSlider(
            serenityCooldownSlider,
            serenityBackground,
            SerenityCooldownRemaining,
            serenityCooldown
        );
    }

    private void UpdateSlider(
        Slider slider,
        Image background,
        float remaining,
        float cooldown)
    {
        if (slider != null)
        {
            slider.value = cooldown > 0f
                ? 1f - (remaining / cooldown)
                : 1f;
        }

        if (background != null)
        {
            background.color = remaining > 0f
                ? cooldownColor
                : readyColor;
        }
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
