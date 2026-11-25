
using UnityEngine;

public class IceWand : SakeroWand
{
    [Header("Ice Projectile")]
    [SerializeField] private IceWandProjectile projectilePrefab;

    [Header("Deep Freeze - Left Upgrade")]
    [SerializeField, Range(0f, 1f)]
    private float slowMultiplierPerHit = 0.85f;

    [SerializeField, Range(0f, 1f)]
    private float minimumSlowMultiplier = 0.25f;

    [SerializeField, Min(0f)]
    private float bonusDamageAtFullSlow = 1f;

    [Header("Frozen Shatter - Right Upgrade")]
    [SerializeField, Range(0f, 1f)]
    private float freezeChance = 0.2f;

    [SerializeField, Min(0f)]
    private float freezeDuration = 1.5f;

    [SerializeField, Min(0)]
    private int shardCount = 3;

    [SerializeField, Min(0f)]
    private float shardRadius = 4f;

    [SerializeField, Min(0f)]
    private float shardDamageMultiplier = 0.5f;

    private bool hasLeftUpgrade;
    private bool hasRightUpgrade;

    public void SetLeftUpgradeEnabled(bool enabled)
    {
        hasLeftUpgrade = enabled;
    }

    public void SetRightUpgradeEnabled(bool enabled)
    {
        hasRightUpgrade = enabled;
    }

    protected override void AttackTarget(AttackableHealth target)
    {
        if (projectilePrefab == null || target == null)
            return;

        Vector3 spawnPosition = GetProjectileSpawnPosition(target);

        GameObject projectileObject =
            ObjectPoolManager.Instance != null
                ? ObjectPoolManager.Instance.GetObject(
                    projectilePrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    projectilePrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                );

        if (projectileObject == null)
            return;

        IceWandProjectile projectile =
            projectileObject.GetComponent<IceWandProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                target,
                CurrentDamage,

                hasLeftUpgrade,
                slowMultiplierPerHit,
                minimumSlowMultiplier,
                bonusDamageAtFullSlow,

                hasRightUpgrade,
                freezeChance,
                freezeDuration,
                projectilePrefab,
                shardCount,
                shardRadius,
                CurrentDamage * shardDamageMultiplier
            );
        }
        else
        {
            Debug.LogWarning(
                "The projectile prefab is missing IceWandProjectile."
            );

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(projectileObject);
            else
                Destroy(projectileObject);
        }
    }
}
