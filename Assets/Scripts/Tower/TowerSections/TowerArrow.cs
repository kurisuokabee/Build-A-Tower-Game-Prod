
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerArrow : TowerSection
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile arrowPrefab;
    [SerializeField] private int projectileCount = 1;

    [Header("Arrow Spread")]
    [SerializeField] private float angleGap = 15f;

    [Header("Barrage Arrow")]
    [SerializeField] private float barrageShotDelay = 0.15f;

    [Header("Extra Arrow Upgrades")]
    [SerializeField] private int extraArrowsPerUpgrade = 2;

    [Header("Arrow Damage Penalties")]
    [SerializeField, Range(0f, 1f)]
    private float extraArrowDamageMultiplier = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float barrageDamageMultiplier = 0.5f;

    private TowerSectionUpgradeTree upgradeTree;

    private bool IsPiercing =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Left;

    private bool IsReturning =>
        IsPiercing &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left1;

    private bool IsBarrage =>
        IsPiercing &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Left2;

    private bool IsHoming =>
        upgradeTree != null &&
        upgradeTree.SelectedBranch ==
        TowerSectionUpgradeTree.UpgradeChoice.Right &&
        upgradeTree.SelectedSpecialization ==
        TowerSectionUpgradeTree.UpgradeChoice.Right1;

    private int TotalProjectileCount
    {
        get
        {
            int count = projectileCount;

            if (upgradeTree == null)
                return count;

            if (upgradeTree.SelectedBranch ==
                TowerSectionUpgradeTree.UpgradeChoice.Right)
            {
                count += extraArrowsPerUpgrade;

                if (upgradeTree.SelectedSpecialization ==
                    TowerSectionUpgradeTree.UpgradeChoice.Right2)
                {
                    count += extraArrowsPerUpgrade;
                }
            }

            return count;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        upgradeTree = GetComponent<TowerSectionUpgradeTree>();
    }

    protected override List<Enemy> FindTargets()
    {
        return FindNearestEnemies(1);
    }

    protected override void Attack(List<Enemy> targets)
    {
        if (firePoint == null || arrowPrefab == null ||
            targets == null || targets.Count == 0 || targets[0] == null)
            return;

        Enemy target = targets[0];
        int arrowCount = TotalProjectileCount;

        float damagePerArrow = Damage;

        // Extra arrows deal reduced damage.
        if (arrowCount > projectileCount)
            damagePerArrow *= extraArrowDamageMultiplier;

        if (IsBarrage)
        {
            int totalArrows = arrowCount + extraArrowsPerUpgrade;

            // Barrage arrows also deal reduced damage.
            damagePerArrow *= barrageDamageMultiplier;

            StartCoroutine(FireBarrage(target, totalArrows, damagePerArrow));
        }
        else
        {
            FireSpreadArrows(target, arrowCount, damagePerArrow);
        }
    }

    private IEnumerator FireBarrage(
        Enemy target,
        int totalArrows,
        float damagePerArrow)
    {
        for (int i = 0; i < totalArrows; i++)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                yield break;

            float angleOffset = GetAngleOffset(i, totalArrows);
            FireArrow(target, damagePerArrow, angleOffset);

            if (i < totalArrows - 1)
                yield return new WaitForSeconds(barrageShotDelay);
        }
    }

    private void FireSpreadArrows(
        Enemy target,
        int count,
        float damagePerArrow)
    {
        for (int i = 0; i < count; i++)
        {
            float angleOffset = GetAngleOffset(i, count);
            FireArrow(target, damagePerArrow, angleOffset);
        }
    }

    private float GetAngleOffset(int index, int count)
    {
        return (index - (count - 1) / 2f) * angleGap;
    }

    private void FireArrow(
        Enemy target,
        float damage,
        float angleOffset = 0f)
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        ArrowProjectile projectile = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        projectile.Initialize(
            target,
            damage,
            IsPiercing,
            IsReturning,
            firePoint,
            angleOffset,
            IsHoming
        );
    }

    public override void ApplyUpgrade()
    {
        Damage += 1f;
        AttacksPerSecond += 0.1f;
    }
}
