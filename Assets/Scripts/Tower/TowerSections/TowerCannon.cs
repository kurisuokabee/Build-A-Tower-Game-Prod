
using System.Collections.Generic;
using UnityEngine;

public class TowerCannon : TowerSection
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CannonProjectile cannonPrefab;

    [Header("Cannon Settings")]
    [SerializeField] private float areaOfEffect = 2f;
    [SerializeField] private int projectileCount = 1;

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        return FindNearestTargets(projectileCount);
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        if (firePoint == null || cannonPrefab == null)
            return;

        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager is missing from the scene.");
            return;
        }

        foreach (AttackableHealth target in targets)
        {
            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                !target.IsAlive)
            {
                continue;
            }

            GameObject projectileObject =
                ObjectPoolManager.Instance.GetObject(
                    cannonPrefab.gameObject,
                    firePoint.position,
                    Quaternion.identity
                );

            if (projectileObject == null)
                continue;

            CannonProjectile projectile =
                projectileObject.GetComponent<CannonProjectile>();

            if (projectile == null)
            {
                Debug.LogError(
                    "Cannon prefab is missing CannonProjectile."
                );

                ObjectPoolManager.Instance.ReturnObject(projectileObject);
                continue;
            }

            projectile.Initialize(target, EffectiveDamage, areaOfEffect);
        }
    }

    public override void ApplyUpgrade()
    {
        Damage += 1f;
        AttacksPerSecond += 0.1f;
    }
}
