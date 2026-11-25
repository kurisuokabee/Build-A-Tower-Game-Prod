
using System.Collections.Generic;
using UnityEngine;

public class TowerSpear : TowerSection
{
    [Header("Spear Settings")]
    [SerializeField] private float spearReach = 2f;
    [SerializeField] private float verticalHitRange = 0.5f;

    [Header("Stunning Spear")]
    [SerializeField] private float stunDuration = 2f;

    [Header("Longer Spear")]
    [SerializeField] private float additionalReach = 1f;

    private readonly List<AttackableHealth> targetBuffer = new();

    private TowerSectionUpgradeTree upgradeTree;
    private SpearAttackController attackController;

    private int attackCount;

    private bool IsStunning =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Left;

    private bool IsLonger =>
        IsStunning &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left1;

    private bool IsRuthless =>
        IsStunning &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left2;

    private bool IsSynergy =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right;

    public bool GrantsAdjacentUpgrade =>
        IsSynergy &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right1;

    private bool IsRelentless =>
        IsSynergy &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right2;

    protected override bool AttackWithoutTargets => IsRelentless;

    private float EffectiveReach =>
        spearReach + (IsLonger ? additionalReach : 0f);

    protected override void Awake()
    {
        base.Awake();

        upgradeTree = GetComponent<TowerSectionUpgradeTree>();
        attackController = GetComponent<SpearAttackController>();

        if (attackController == null)
        {
            Debug.LogError(
                "TowerSpear requires SpearAttackController.",
                this
            );
        }
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        targetBuffer.Clear();

        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        float originX = transform.position.x;
        float originY = transform.position.y;

        float reach = EffectiveReach;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive)
            {
                continue;
            }

            Vector3 position = target.transform.position;

            if (Mathf.Abs(position.x - originX) <= reach &&
                Mathf.Abs(position.y - originY) <= verticalHitRange)
            {
                targetBuffer.Add(target);
            }
        }

        return targetBuffer;
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        if (attackController == null ||
            attackController.IsAttacking)
        {
            return;
        }

        if (IsSynergy && !IsTriggeredAttack)
        {
            TowerBuilding.Instance?.TriggerAdjacentSections(this);
        }

        bool ruthlessAttack = attackCount == 2;

        attackController.StartAttack(
            EffectiveReach,
            EffectiveDamage,
            IsRuthless,
            ruthlessAttack,
            IsStunning,
            stunDuration
        );

        attackCount++;

        if (attackCount >= 3)
            attackCount = 0;
    }

    protected override void ApplyUpgradeStats()
    {
        Damage += 1.5f;
        AttacksPerSecond += 0.1f;
    }
}
