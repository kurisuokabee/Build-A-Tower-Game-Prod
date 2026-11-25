
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public abstract class TowerSection : MonoBehaviour
{
    [Header("Section Size")]
    public float height;
    public float width;

    [Header("Section Data")]
    [SerializeField] private TowerSectionData sectionData;

    [Header("Place Effect")]
    [SerializeField] private float placeEffectDuration = 0.25f;
    [SerializeField] private float placeEffectOvershoot = 1.3f;

    [SerializeField] SpriteRenderer spriteRenderer;
    public TowerSectionData Data => sectionData;

    public float AttackRange { get; protected set; }
    public float AttacksPerSecond { get; protected set; }
    public float Damage { get; protected set; }

    public int RequiredLevel =>
        sectionData != null ? sectionData.requiredLevel : 1;

    public bool IsUnlocked =>
        PlayerEconomy.Instance != null &&
        PlayerEconomy.Instance.Level >= RequiredLevel;

    public bool CanAfford =>
        PlayerEconomy.Instance != null &&
        PlayerEconomy.Instance.Blocks >= BlockCost;

    private float attackTimer;
    private bool isTriggeredAttack;

    private int temporaryLevelBonus;
    private float temporaryDamageBonus;
    private float temporaryAttackSpeedBonus;

    protected bool IsTriggeredAttack => isTriggeredAttack;

    public int TemporaryLevelBonus => temporaryLevelBonus;
    protected virtual bool AttackWithoutTargets => false;
    protected virtual float AttackSpeedMultiplier => 1f;

    protected virtual void Awake()
    {
        if (sectionData == null)
            return;

        AttackRange = sectionData.attackRange;
        AttacksPerSecond = sectionData.attacksPerSecond;
        Damage = sectionData.damage;
    }

    
    protected virtual void Update()
    {
        if (EffectiveAttacksPerSecond <= 0f)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        IReadOnlyList<AttackableHealth> targets = FindTargets();

        if ((targets == null || targets.Count == 0) &&
            !AttackWithoutTargets)
        {
            return;
        }

        Attack(targets);
        attackTimer = 1f / EffectiveAttacksPerSecond;
    }

    public void ApplyWandImpact(AttackableHealth target)
    {
        if (TowerImpactEffect.Instance == null)
            return;

        TowerImpactEffect.Instance.HandleImpact(this, target);
    }

    
    public void PlayPlaceEffect()
    {
        StopCoroutine(nameof(PlaceEffect));
        StartCoroutine(PlaceEffect());
    }

    private IEnumerator PlaceEffect()
    {   
        spriteRenderer.sprite = sectionData.icon;

        Vector3 originalScale = transform.localScale;

        transform.localScale = Vector3.zero;

        float timer = 0f;
        float halfDuration = placeEffectDuration * 0.5f;

        // Scale from 0 to 1.2
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                originalScale * Mathf.Lerp(1f, placeEffectOvershoot, t);

            yield return null;
        }

        timer = 0f;

        // Scale from 1.2 back to 1
        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale =
                originalScale * Mathf.Lerp(placeEffectOvershoot, 1f, t);

            yield return null;
        }

        transform.localScale = originalScale;
    }
    
    public bool TriggerAttackImmediately()
    {
        if (!isActiveAndEnabled)
            return false;

        isTriggeredAttack = true;

        try
        {
            Attack(FindTargets());
        }
        finally
        {
            isTriggeredAttack = false;
        }

        return true;
    }

    protected IReadOnlyList<AttackableHealth> FindNearestTargets(int count)
    {
        return AttackableTargetFinder.FindNearestTargets(
            transform.position,
            count,
            AttackRange
        );
    }

    protected AttackableHealth FindNearestTarget()
    {
        return AttackableTargetFinder.FindNearestTarget(
            transform.position,
            AttackRange
        );
    }

    public int BlockCost
    {
        get
        {
            if (sectionData == null)
                return 0;

            if (TowerSectionUpgrade.Instance == null)
                return sectionData.cost;

            return TowerSectionUpgrade.Instance.GetBuildCost(
                sectionData.towerUpgradeID,
                sectionData.cost
            );
        }
    }

    
    // Permanent upgrade.
    public void ApplyUpgrade()
    {
        RemoveTemporaryBonus();

        ApplyUpgradeStats();

        ApplyTemporaryBonus();
    }

    // Temporary upgrade level from Upgrading Spear.
    public void SetTemporaryLevelBonus(int levels)
    {
        RemoveTemporaryBonus();

        temporaryLevelBonus = Mathf.Max(0, levels);

        ApplyTemporaryBonus();
    }

    private void ApplyTemporaryBonus()
    {
        if (temporaryLevelBonus <= 0)
            return;

        float originalDamage = Damage;
        float originalAttackSpeed = AttacksPerSecond;

        // Measure what one normal upgrade adds.
        ApplyUpgradeStats();

        float damagePerLevel = Damage - originalDamage;
        float attackSpeedPerLevel =
            AttacksPerSecond - originalAttackSpeed;

        // Restore permanent stats.
        Damage = originalDamage;
        AttacksPerSecond = originalAttackSpeed;

        // Apply the temporary equivalent.
        temporaryDamageBonus =
            damagePerLevel * temporaryLevelBonus;

        temporaryAttackSpeedBonus =
            attackSpeedPerLevel * temporaryLevelBonus;

        Damage += temporaryDamageBonus;
        AttacksPerSecond += temporaryAttackSpeedBonus;
    }

    private void RemoveTemporaryBonus()
    {
        Damage -= temporaryDamageBonus;
        AttacksPerSecond -= temporaryAttackSpeedBonus;

        temporaryDamageBonus = 0f;
        temporaryAttackSpeedBonus = 0f;
    }

    public float EffectiveDamage
    {
        get
        {
            float multiplier =
                TowerUtilities.Instance != null &&
                TowerUtilities.Instance.IsRageActive
                    ? TowerUtilities.Instance.RageMultiplier
                    : 1f;

            return Damage * multiplier;
        }
    }

    public float EffectiveAttacksPerSecond
    {
        get
        {
            float rageMultiplier =
                TowerUtilities.Instance != null &&
                TowerUtilities.Instance.IsRageActive
                    ? TowerUtilities.Instance.RageMultiplier
                    : 1f;

            return AttacksPerSecond *
                rageMultiplier *
                AttackSpeedMultiplier;
        }
    }

    protected abstract IReadOnlyList<AttackableHealth> FindTargets();

    protected abstract void Attack(
        IReadOnlyList<AttackableHealth> targets
    );

    protected abstract void ApplyUpgradeStats();
}
