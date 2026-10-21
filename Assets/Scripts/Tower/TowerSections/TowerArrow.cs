using System.Collections.Generic;
using UnityEngine;

public class TowerArrow : TowerSection
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile arrowPrefab;

    protected override List<Enemy> FindTargets()
    {
        return FindNearestEnemies(ProjectileCount);
    }

    protected override void Attack(List<Enemy> targets)
    {
        if (firePoint == null || arrowPrefab == null)
            return;

        foreach (Enemy target in targets)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
                continue;

            ArrowProjectile projectile = Instantiate(
                arrowPrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(target, Damage);
        }
    }

    public override void ApplyUpgrade(bool isSpecialUpgrade)
    {
        if (isSpecialUpgrade)
        {
            ProjectileCount += 1;
        }
        else
        {
            Damage += 1f;
            AttacksPerSecond =+ 1f;
    
        }

        
    }
}