
using UnityEngine;

public class CannonFiringController : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CannonProjectile cannonPrefab;

    [Header("Cannon Settings")]
    [SerializeField] private float areaOfEffect = 2f;
    [SerializeField] private int projectileCount = 1;

    [Header("Nuke Cannon")]
    [SerializeField] private float nukeDamageMultiplier = 2f;
    [SerializeField] private float nukeExplosionMultiplier = 2f;

    [Header("Cluster Cannon")]
    [SerializeField, Range(0.1f, 1f)]
    private float clusterDamageMultiplier = 0.5f;

    [SerializeField, Range(0.1f, 1f)]
    private float clusterExplosionMultiplier = 0.65f;

    [SerializeField] private float clusterSpread = 1.5f;

    [Header("Trebuchet")]
    [SerializeField] private float stunDuration = 2f;

    [Header("Trebuchet II")]
    [SerializeField, Range(0f, 1f)]
    private float shardDamageMultiplier = 0.5f;

    public void Fire(
        AttackableHealth target,
        float damage,
        bool missile,
        bool nuke,
        bool cluster,
        bool trebuchet,
        bool trebuchetII,
        bool bombardment)
    {
        if (target == null ||
            !target.isActiveAndEnabled ||
            !target.IsAlive)
        {
            return;
        }

        if (firePoint == null || cannonPrefab == null)
            return;

        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError(
                "ObjectPoolManager is missing from the scene.",
                this
            );

            return;
        }

        float explosionRadius = areaOfEffect;

        if (nuke)
        {
            damage *= nukeDamageMultiplier;
            explosionRadius *= nukeExplosionMultiplier;
        }

        for (int i = 0; i < projectileCount; i++)
        {
            SpawnProjectile(
                target,
                damage,
                explosionRadius,
                missile,
                cluster,
                trebuchet,
                trebuchetII,
                bombardment
            );
        }
    }

    private void SpawnProjectile(
        AttackableHealth target,
        float damage,
        float explosionRadius,
        bool missile,
        bool cluster,
        bool trebuchet,
        bool trebuchetII,
        bool bombardment)
    {
        GameObject projectileObject =
            ObjectPoolManager.Instance.GetObject(
                cannonPrefab.gameObject,
                firePoint.position,
                Quaternion.identity
            );

        if (projectileObject == null)
            return;

        CannonProjectile projectile =
            projectileObject.GetComponent<CannonProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "Cannon prefab is missing CannonProjectile.",
                this
            );

            ObjectPoolManager.Instance.ReturnObject(projectileObject);
            return;
        }

        projectile.Initialize(
            target,
            cannonPrefab.gameObject,
            damage,
            explosionRadius,
            missile,
            cluster,
            trebuchet,
            trebuchetII,
            stunDuration,
            clusterDamageMultiplier,
            clusterExplosionMultiplier,
            clusterSpread,
            shardDamageMultiplier,
            bombardment
        );
    }
}
