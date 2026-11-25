using UnityEngine;

public class TowerSectionUpgradeIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer normalSprite;
    [SerializeField] private SpriteRenderer upgradeSprite;

    [Header("Animation")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float minAlpha = 0f;
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 1f;

    private bool canUpgrade;

    private void Awake()
    {
        SetAlpha(upgradeSprite, 0f);
    }

    private void Update()
    {
        if (!canUpgrade)
            return;

        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        SetAlpha(upgradeSprite, alpha);
    }

    public void SetUpgradeAvailable(bool available)
    {
        canUpgrade = available;

        if (!available)
            SetAlpha(upgradeSprite, 0f);
    }

    private void SetAlpha(SpriteRenderer sprite, float alpha)
    {
        Color color = sprite.color;
        color.a = alpha;
        sprite.color = color;
    }
}