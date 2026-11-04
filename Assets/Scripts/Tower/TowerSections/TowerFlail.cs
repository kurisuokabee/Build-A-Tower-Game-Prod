using System.Collections.Generic;
using UnityEngine;

public class TowerFlail : TowerSection
{
    private TowerSectionUpgradeTree upgradeTree;
    private FlailWeaponController weaponController;

    private bool IsMultiFlail =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Left;

    private bool IsCharging =>
        IsMultiFlail &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left1;

    private bool IsRoulette =>
        IsMultiFlail &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left2;

    private bool IsSword =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right;

    private bool IsMultiSword =>
        IsSword &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right1;

    public bool GrantsAlliance =>
        IsSword &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right2;

    private bool HasSecondWeapon =>
        IsMultiFlail || IsMultiSword;

    protected override void Awake()
    {
        base.Awake();

        upgradeTree = GetComponent<TowerSectionUpgradeTree>();
        weaponController = GetComponent<FlailWeaponController>();

        if (weaponController == null)
        {
            Debug.LogError(
                "TowerFlail requires FlailWeaponController.",
                this
            );
        }
    }

    protected override void Start()
    {
        base.Start();
    }

    private void OnEnable()
    {
        TowerBuilding.Instance?.RefreshTemporaryUpgrades();
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        return AttackableTargetRegistry.Targets;
    }

    protected override void Update()
    {
        if (weaponController == null)
            return;

        weaponController.Tick(
            IsCharging,
            HasSecondWeapon,
            IsSword,
            IsRoulette,
            EffectiveDamage
        );
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        // Contact damage is handled by FlailWeaponController.
    }

    protected override void ApplyUpgradeStats()
    {
        Damage += 1f;
    }
}
