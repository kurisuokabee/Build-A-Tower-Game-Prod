
using UnityEngine;

public class AttackableSlow : MonoBehaviour
{
    [Header("Slow Visual")]
    [SerializeField] private Color slowedColor = Color.lightSkyBlue;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private float slowMultiplier = 1f;
    private float slowDuration;
    private float slowTimer;

    public float CurrentSpeedMultiplier =>
        slowTimer > 0f ? slowMultiplier : 1f;

    public bool IsSlowed => slowTimer > 0f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    private void Update()
    {
        if (slowTimer <= 0f)
            return;

        slowTimer -= Time.deltaTime;

        if (slowTimer <= 0f)
        {
            slowTimer = 0f;
            slowMultiplier = 1f;
            slowDuration = 0f;

            UpdateSlowVisual();
        }
    }

    private void OnEnable()
    {
        ResetSlow();
    }

    private void OnDisable()
    {
        ResetSlow();
    }

    public void ApplySlow(float multiplier, float duration)
    {
        multiplier = Mathf.Clamp01(multiplier);

        if (slowTimer <= 0f || multiplier < slowMultiplier)
            slowMultiplier = multiplier;

        slowDuration = Mathf.Max(slowDuration, duration);
        slowTimer = slowDuration;

        UpdateSlowVisual();
    }

    public void ApplyStackingSlow(
        float multiplierPerHit,
        float minimumMultiplier,
        float duration)
    {
        multiplierPerHit = Mathf.Clamp01(multiplierPerHit);
        minimumMultiplier = Mathf.Clamp01(minimumMultiplier);

        float currentMultiplier = CurrentSpeedMultiplier;

        slowMultiplier = Mathf.Max(
            minimumMultiplier,
            currentMultiplier * multiplierPerHit
        );

        slowDuration = Mathf.Max(slowDuration, duration);
        slowTimer = slowDuration;

        UpdateSlowVisual();
    }

    private void UpdateSlowVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = IsSlowed
            ? slowedColor
            : originalColor;
    }

    private void ResetSlow()
    {
        slowMultiplier = 1f;
        slowDuration = 0f;
        slowTimer = 0f;

        UpdateSlowVisual();
    }
}
