
using System.Collections.Generic;
using UnityEngine;

public class TowerCannon : TowerSection
{
    [Header("Attack Settings")]
    [SerializeField, Range(0.1f, 1f)]
    private float nukeAttackSpeedMultiplier = 0.5f;

    private TowerSectionUpgradeTree upgradeTree;
    private CannonFiringController firingController;

    private AttackableHealth forcedTarget;

    private readonly List<AttackableHealth> singleTarget = new(1);

    private bool IsMissile =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Left;

    private bool IsNuke =>
        IsMissile &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left1;

    private bool IsCluster =>
        IsMissile &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left2;

    private bool IsTrebuchet =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right;

    private bool IsTrebuchetII =>
        IsTrebuchet &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right1;

    private bool IsBombardment =>
        IsTrebuchet &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right2;

    protected override float AttackSpeedMultiplier =>
        IsNuke ? nukeAttackSpeedMultiplier : 1f;

    protected override void Awake()
    {
        base.Awake();

        upgradeTree = GetComponent<TowerSectionUpgradeTree>();
        firingController = GetComponent<CannonFiringController>();
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        if (forcedTarget != null &&
            forcedTarget.isActiveAndEnabled &&
            forcedTarget.IsAlive)
        {
            singleTarget.Clear();
            singleTarget.Add(forcedTarget);

            return singleTarget;
        }

        if (IsBombardment)
        {
            AttackableHealth target = FindHighestMaxHealthTarget();

            singleTarget.Clear();

            if (target != null)
                singleTarget.Add(target);

            return singleTarget;
        }

        return FindNearestTargets(1);
    }

    private AttackableHealth FindHighestMaxHealthTarget()
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        AttackableHealth bestTarget = null;

        float highestMaxHealth = -1f;
        float closestDistance = float.MaxValue;
        float rangeSquared = AttackRange * AttackRange;

        Vector2 origin = transform.position;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive)
            {
                continue;
            }

            float distanceSquared =
                ((Vector2)target.transform.position - origin).sqrMagnitude;

            if (distanceSquared > rangeSquared)
                continue;

            float maxHealth = target.MaxHealth;

            if (maxHealth > highestMaxHealth ||
                (Mathf.Approximately(maxHealth, highestMaxHealth) &&
                 distanceSquared < closestDistance))
            {
                bestTarget = target;
                highestMaxHealth = maxHealth;
                closestDistance = distanceSquared;
            }
        }

        return bestTarget;
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        if (firingController == null || targets == null)
            return;

        AttackableHealth bombardmentTarget = null;

        foreach (AttackableHealth target in targets)
        {
            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                !target.IsAlive)
            {
                continue;
            }

            firingController.Fire(
                target,
                EffectiveDamage,
                IsMissile,
                IsNuke,
                IsCluster,
                IsTrebuchet,
                IsTrebuchetII,
                IsBombardment
            );

            if (IsBombardment)
                bombardmentTarget = target;
        }

        if (bombardmentTarget != null && !IsTriggeredAttack)
            TriggerAdjacentCannons(bombardmentTarget);
    }

    private void TriggerAdjacentCannons(AttackableHealth target)
    {
        TowerBuilding building = TowerBuilding.Instance;

        if (building == null)
            return;

        IReadOnlyList<TowerSection> sections = building.Sections;

        int index = -1;

        for (int i = 0; i < sections.Count; i++)
        {
            if (sections[i] == this)
            {
                index = i;
                break;
            }
        }

        if (index < 0)
            return;

        if (index > 0 &&
            sections[index - 1] is TowerCannon below &&
            below.isActiveAndEnabled)
        {
            below.TriggerBombardmentAttack(target);
        }

        if (index < sections.Count - 1 &&
            sections[index + 1] is TowerCannon above &&
            above.isActiveAndEnabled)
        {
            above.TriggerBombardmentAttack(target);
        }
    }

    private void TriggerBombardmentAttack(AttackableHealth target)
    {
        if (target == null || !target.IsAlive)
            return;

        forcedTarget = target;

        try
        {
            TriggerAttackImmediately();
        }
        finally
        {
            forcedTarget = null;
        }
    }

    protected override void ApplyUpgradeStats()
    {
        Damage += 0.25f;
        AttacksPerSecond += 0.15f;
    }
}
