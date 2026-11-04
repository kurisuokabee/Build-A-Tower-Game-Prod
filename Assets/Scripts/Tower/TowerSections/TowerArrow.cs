
using System.Collections.Generic;
using UnityEngine;

public class TowerArrow : TowerSection
{
    private TowerSectionUpgradeTree upgradeTree;
    private ArrowFiringController firingController;
    private readonly List<AttackableHealth> homingTargetBuffer = new();

    private bool IsPiercing =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Left;

    private bool IsReturning =>
        IsPiercing &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left1;

    private bool IsFollow =>
        IsPiercing &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left2;

    private bool IsHoming =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right1;

    private bool IsBarrage =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right2;

    private bool HasExtraArrows =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right;

    protected override void Awake()
    {
        base.Awake();

        upgradeTree = GetComponent<TowerSectionUpgradeTree>();
        firingController = GetComponent<ArrowFiringController>();

        if (firingController == null)
        {
            Debug.LogError(
                "TowerArrow requires ArrowFiringController.",
                this
            );
        }
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        if (IsFollow)
        {
            AttackableHealth cursorTarget = FindCursorTarget();

            if (cursorTarget == null)
                return System.Array.Empty<AttackableHealth>();

            return new AttackableHealth[] { cursorTarget };
        }

        if (IsHoming)
            return FindHomingTargets();

        AttackableHealth target = FindNearestTarget();

        if (target == null)
            return System.Array.Empty<AttackableHealth>();

        return new AttackableHealth[] { target };
    }

    private AttackableHealth FindFarthestTarget()
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        AttackableHealth farthestTarget = null;

        float farthestDistance = -1f;
        float attackRangeSquared = AttackRange * AttackRange;

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

            if (distanceSquared > attackRangeSquared)
                continue;

            if (distanceSquared > farthestDistance)
            {
                farthestDistance = distanceSquared;
                farthestTarget = target;
            }
        }

        return farthestTarget;
    }

    private AttackableHealth FindCursorTarget()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return null;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] colliders =
            Physics2D.OverlapPointAll(mousePosition);

        foreach (Collider2D col in colliders)
        {
            if (col == null)
                continue;

            AttackableHealth target =
                col.GetComponentInParent<AttackableHealth>();

            if (target != null &&
                target.isActiveAndEnabled &&
                target.IsAlive)
            {
                return target;
            }
        }

        return null;
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        if (firingController == null ||
            targets == null ||
            targets.Count == 0)
        {
            return;
        }

        if (IsHoming)
        {
            firingController.FireHoming(
                targets,
                HasExtraArrows,
                EffectiveDamage
            );

            return;
        }

        if (targets[0] == null)
            return;

        firingController.Fire(
            targets[0],
            IsPiercing,
            IsReturning,
            false,
            IsBarrage,
            HasExtraArrows,
            EffectiveDamage
        );
    }

    private IReadOnlyList<AttackableHealth> FindHomingTargets()
    {
        homingTargetBuffer.Clear();

        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        Vector2 origin = transform.position;
        float attackRangeSquared = AttackRange * AttackRange;

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

            if (distanceSquared <= attackRangeSquared)
                homingTargetBuffer.Add(target);
        }

        // Sort from farthest to nearest.
        homingTargetBuffer.Sort((a, b) =>
        {
            float distanceA =
                ((Vector2)a.transform.position - origin).sqrMagnitude;

            float distanceB =
                ((Vector2)b.transform.position - origin).sqrMagnitude;

            return distanceB.CompareTo(distanceA);
        });

        return homingTargetBuffer;
    }

    protected override void ApplyUpgradeStats()
    {
        Damage += 1f;
        AttacksPerSecond += 0.1f;
    }
}
