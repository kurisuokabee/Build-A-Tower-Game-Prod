
using System.Collections.Generic;
using UnityEngine;

public abstract class TowerSection : MonoBehaviour
{
    [Header("Section Size")]
    public float height;
    public float width;

    [Header("Section Data")]
    [SerializeField] private TowerSectionData sectionData;

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

    protected virtual void Awake()
    {
        if (sectionData == null)
            return;

        AttackRange = sectionData.attackRange;
        AttacksPerSecond = sectionData.attacksPerSecond;
        Damage = sectionData.damage;
    }

    protected virtual void Start()
    {
        ApplyGlobalUpgrades();
    }

    protected virtual void Update()
    {
        if (EffectiveAttacksPerSecond <= 0f)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        IReadOnlyList<AttackableHealth> targets = FindTargets();

        if (targets == null || targets.Count == 0)
            return;

        Attack(targets);
        attackTimer = 1f / EffectiveAttacksPerSecond;
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

    public void ApplyGlobalUpgrades()
    {
        if (TowerSectionUpgrade.Instance == null || sectionData == null)
            return;

        ApplyUpgrade();
    }

    public float EffectiveDamage
    {
        get
        {
            float multiplier = TowerUtilities.Instance != null &&
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
            float multiplier = TowerUtilities.Instance != null &&
                            TowerUtilities.Instance.IsRageActive
                ? TowerUtilities.Instance.RageMultiplier
                : 1f;

            return AttacksPerSecond * multiplier;
        }
    }

    protected abstract IReadOnlyList<AttackableHealth> FindTargets();

    protected abstract void Attack(
        IReadOnlyList<AttackableHealth> targets
    );

    public abstract void ApplyUpgrade();
}
