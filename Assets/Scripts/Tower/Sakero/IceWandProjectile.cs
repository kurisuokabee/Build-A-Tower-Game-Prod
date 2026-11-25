
using UnityEngine;

public class IceWandProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.15f;

    [Header("Ice Effect")]
    [SerializeField, Range(0f, 1f)] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 3f;

    private AttackableHealth target;
    private float damage;

    // Deep Freeze
    private bool deepFreezeEnabled;
    private float slowMultiplierPerHit;
    private float minimumSlowMultiplier;
    private float bonusDamageAtFullSlow;

    // Frozen Shatter
    private bool frozenShatterEnabled;
    private float freezeChance;
    private float freezeDuration;
    private IceWandProjectile shardPrefab;
    private int shardCount;
    private float shardRadius;
    private float shardDamageMultiplier;

    // Prevents shards from triggering Frozen Shatter.
    private bool isShard;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(
        AttackableHealth newTarget,
        float newDamage,
        bool hasDeepFreeze,
        float newSlowMultiplierPerHit,
        float newMinimumSlowMultiplier,
        float newBonusDamageAtFullSlow,
        bool hasFrozenShatter,
        float newFreezeChance,
        float newFreezeDuration,
        IceWandProjectile newShardPrefab,
        int newShardCount,
        float newShardRadius,
        float newShardDamageMultiplier)
    {
        target = newTarget;
        damage = newDamage;

        deepFreezeEnabled = hasDeepFreeze;
        slowMultiplierPerHit = newSlowMultiplierPerHit;
        minimumSlowMultiplier = newMinimumSlowMultiplier;
        bonusDamageAtFullSlow = newBonusDamageAtFullSlow;

        frozenShatterEnabled = hasFrozenShatter;
        freezeChance = newFreezeChance;
        freezeDuration = newFreezeDuration;
        shardPrefab = newShardPrefab;
        shardCount = newShardCount;
        shardRadius = newShardRadius;
        shardDamageMultiplier = newShardDamageMultiplier;

        isShard = false;
    }

    // Used when spawning shards from a frozen enemy.
    public void InitializeShard(
        AttackableHealth newTarget,
        float newDamage)
    {
        target = newTarget;
        damage = newDamage;

        deepFreezeEnabled = false;
        frozenShatterEnabled = false;
        isShard = true;
    }

    private void Update()
    {
        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        Vector3 targetPosition = target.transform.position;
        targetPosition.z = transform.position.z;

        Vector3 direction = targetPosition - transform.position;
        RotateTowardsTarget(direction);
        if (direction.sqrMagnitude <= hitDistance * hitDistance)
        {
            HitTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    private void HitTarget()
    {
        AttackableSlow slow = target.GetComponent<AttackableSlow>();

        float finalDamage = damage;

        if (slow != null)
        {
            if (deepFreezeEnabled && !isShard)
            {
                slow.ApplyStackingSlow(
                    slowMultiplierPerHit,
                    minimumSlowMultiplier,
                    slowDuration
                );

                float slowAmount = 1f - slow.CurrentSpeedMultiplier;

                finalDamage *= 1f +
                    slowAmount * bonusDamageAtFullSlow;
            }
            else
            {
                slow.ApplySlow(slowMultiplier, slowDuration);
            }
        }

        target.TakeDamage(finalDamage);

        // The target may have died from this hit.
        if (target == null || !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        if (frozenShatterEnabled &&
            !isShard &&
            slow != null &&
            slow.IsSlowed &&
            Random.value <= freezeChance)
        {
            target.ApplyStun(freezeDuration);

            IceFreezeEffect freezeEffect =
                target.GetComponent<IceFreezeEffect>();

            if (freezeEffect == null)
                freezeEffect = target.gameObject.AddComponent<IceFreezeEffect>();

            freezeEffect.Configure(
                target,
                freezeDuration,
                shardPrefab,
                shardCount,
                shardRadius,
                damage * shardDamageMultiplier
            );
        }

        ReturnToPool();
    }

    private void RotateTowardsTarget(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );
    }
    private void ReturnToPool()
    {
        target = null;
        damage = 0f;

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
