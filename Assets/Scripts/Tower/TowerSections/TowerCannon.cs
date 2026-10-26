
using System.Collections.Generic;
using UnityEngine;

public class TowerCannon : TowerSection
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CannonProjectile cannonPrefab;
    private float areaOfEffect = 2;
    private int ProjectileCount;
    protected override List<Enemy> FindTargets()
    {
        return FindNearestEnemies(ProjectileCount);
    }

    protected override void Attack(List<Enemy> targets)
    {
        if (firePoint == null || cannonPrefab == null)
            return;

        foreach (Enemy target in targets)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                continue;

            CannonProjectile projectile = Instantiate(
                cannonPrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(target, Damage, areaOfEffect);
        }
    }

    public override void ApplyUpgrade()
    {
        Damage += 1f;
        AttacksPerSecond =+ 1f;
    }
}
