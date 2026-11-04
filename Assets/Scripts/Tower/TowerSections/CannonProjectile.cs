
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CannonProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float despawnTime = 4f;

    [Header("Trebuchet Shards")]
    [SerializeField] private float shardSpeed = 7f;
    [SerializeField] private float shardLifetime = 1.5f;

    private Rigidbody2D rb;
    private float originalGravityScale;

    private float damage;
    private float areaOfEffect;
    private float lifetime;
    private float flightTime;
    private float stunDuration;

    private float clusterDamageMultiplier;
    private float clusterExplosionMultiplier;
    private float clusterSpread;
    private float shardDamageMultiplier;

    private Vector2 destination;

    private GameObject projectilePrefab;

    private bool initialized;
    private bool hasExploded;
    private bool hasSplit;

    private bool isMissile;
    private bool isCluster;
    private bool isTrebuchet;
    private bool isTrebuchetII;
    private bool isShard;
    private bool isBombardment;
    private bool autoExplodeAtDestination;

    private AttackableHealth assignedTarget;
    private AttackableHealth ignoredTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    public void Initialize(
        AttackableHealth target,
        GameObject projectilePrefab,
        float damage,
        float aoe,
        bool missile,
        bool cluster,
        bool trebuchet,
        bool trebuchetII,
        float stunDuration,
        float clusterDamageMultiplier,
        float clusterExplosionMultiplier,
        float clusterSpread,
        float shardDamageMultiplier,
        bool bombardment)
    {
        ResetProjectile();

        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        this.projectilePrefab = projectilePrefab;
        this.damage = damage;
        areaOfEffect = aoe;

        isMissile = missile;
        isCluster = cluster;
        isTrebuchet = trebuchet;
        isTrebuchetII = trebuchetII;
        isBombardment = bombardment;

        assignedTarget = target;

        this.stunDuration = stunDuration;

        this.clusterDamageMultiplier = clusterDamageMultiplier;
        this.clusterExplosionMultiplier = clusterExplosionMultiplier;
        this.clusterSpread = clusterSpread;
        this.shardDamageMultiplier = shardDamageMultiplier;

        destination = target.transform.position;

        Launch(destination, isMissile);
    }

    private void Launch(Vector2 targetPosition, bool straight)
    {
        lifetime = 0f;

        Vector2 startPosition = rb.position;
        Vector2 direction = targetPosition - startPosition;

        flightTime = Mathf.Max(
            direction.magnitude / Mathf.Max(launchSpeed, 0.01f),
            0.5f
        );

        destination = targetPosition;

        if (straight)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = direction.normalized * launchSpeed;
        }
        else
        {
            rb.gravityScale = originalGravityScale;

            float gravity =
                Physics2D.gravity.y * rb.gravityScale;

            Vector2 velocity = new Vector2(
                direction.x / flightTime,
                (direction.y -
                 0.5f * gravity * flightTime * flightTime)
                / flightTime
            );

            rb.linearVelocity = velocity;
        }

        initialized = true;
    }

    private void ResetProjectile()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = originalGravityScale;

        lifetime = 0f;
        flightTime = 0f;

        initialized = false;
        hasExploded = false;
        hasSplit = false;

        isMissile = false;
        isCluster = false;
        isTrebuchet = false;
        isTrebuchetII = false;
        isShard = false;
        isBombardment = false;
        autoExplodeAtDestination = false;

        projectilePrefab = null;

        assignedTarget = null;
        ignoredTarget = null;
    }

    private void Update()
    {
        if (!initialized || hasExploded)
            return;

        lifetime += Time.deltaTime;

        if (isCluster && !hasSplit &&
            lifetime >= flightTime * 0.5f)
        {
            SplitCluster();
            return;
        }

        if (autoExplodeAtDestination && lifetime >= flightTime)
        {
            Explode();
            return;
        }

        if (lifetime >= (isShard ? shardLifetime : despawnTime))
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hasExploded)
            return;

        AttackableHealth target =
            other.GetComponentInParent<AttackableHealth>();

        // Bombardment only reacts to its assigned target.
        if (isBombardment)
        {
            if (target == null || target != assignedTarget)
                return;
        }
        else if (!IsValidTarget(other))
        {
            return;
        }

        if (isShard)
        {
            HitWithShard(other);
            return;
        }

        if (isTrebuchet)
        {
            HitWithTrebuchet(other);
            return;
        }

        Explode();
    }

    private bool IsValidTarget(Collider2D other)
    {
        return other.GetComponentInParent<AttackableHealth>() != null ||
               other.CompareTag("Debris") ||
               other.transform.root.CompareTag("Debris");
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;
        initialized = false;

        DamageAllInRange(
            transform.position,
            areaOfEffect,
            damage
        );

        ReturnToPool();
    }

    private void DamageAllInRange(
        Vector2 center,
        float radius,
        float damageAmount)
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        float radiusSquared = radius * radius;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                !target.IsAlive)
            {
                continue;
            }

            Vector2 offset =
                (Vector2)target.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
                target.TakeDamage(damageAmount);
        }
    }

    private void SplitCluster()
    {
        if (hasSplit || hasExploded)
            return;

        hasSplit = true;
        initialized = false;

        Vector2 splitPosition = transform.position;

        Vector2[] directions =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        for (int i = 0; i < directions.Length; i++)
        {
            Vector2 childDestination =
                destination + directions[i] * clusterSpread;

            SpawnClusterChild(
                splitPosition,
                childDestination
            );
        }

        ReturnToPool();
    }

    private void SpawnClusterChild(
        Vector2 startPosition,
        Vector2 childDestination)
    {
        CannonProjectile child = GetPooledProjectile(startPosition);

        if (child == null)
            return;

        child.InitializeClusterChild(
            childDestination,
            projectilePrefab,
            damage * clusterDamageMultiplier,
            areaOfEffect * clusterExplosionMultiplier,
            isMissile
        );
    }

    private void InitializeClusterChild(
        Vector2 targetPosition,
        GameObject prefab,
        float childDamage,
        float childAOE,
        bool straight)
    {
        ResetProjectile();

        projectilePrefab = prefab;
        damage = childDamage;
        areaOfEffect = childAOE;

        autoExplodeAtDestination = true;

        Launch(targetPosition, straight);
    }

    private void HitWithTrebuchet(Collider2D other)
    {
        if (hasExploded)
            return;

        hasExploded = true;
        initialized = false;

        AttackableHealth target =
            other.GetComponentInParent<AttackableHealth>();

        if (target != null && target.IsAlive)
        {
            target.TakeDamage(damage);
            target.ApplyStun(stunDuration);

            Debug.Log(
                $"[Trebuchet] Hit {target.name}. Stunned for {stunDuration}s.",
                this
            );
        }

        if (isTrebuchetII)
            SpawnTrebuchetShards(transform.position, target);

        ReturnToPool();
    }

    private void SpawnTrebuchetShards(
        Vector2 position,
        AttackableHealth originalTarget)
    {
        Vector2[] directions =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right
        };

        for (int i = 0; i < directions.Length; i++)
        {
            CannonProjectile shard = GetPooledProjectile(position);

            if (shard == null)
            {
                Debug.LogWarning(
                    $"[Trebuchet II] Failed to spawn shard {i + 1}.",
                    this
                );

                continue;
            }

            shard.InitializeShard(
                directions[i],
                projectilePrefab,
                damage * shardDamageMultiplier,
                stunDuration,
                originalTarget
            );
        }

        Debug.Log("[Trebuchet II] Spawned shards.", this);
    }

    private void InitializeShard(
        Vector2 direction,
        GameObject prefab,
        float shardDamage,
        float stun,
        AttackableHealth originalTarget)
    {
        ResetProjectile();

        projectilePrefab = prefab;
        damage = shardDamage;
        stunDuration = stun;
        ignoredTarget = originalTarget;

        isShard = true;

        rb.gravityScale = 0f;
        rb.linearVelocity = direction.normalized * shardSpeed;

        initialized = true;
    }

    private void HitWithShard(Collider2D other)
    {
        if (hasExploded)
            return;

        AttackableHealth target =
            other.GetComponentInParent<AttackableHealth>();

        if (target != null && target == ignoredTarget)
            return;

        hasExploded = true;
        initialized = false;

        if (target != null && target.IsAlive)
        {
            target.TakeDamage(damage);
            target.ApplyStun(stunDuration);
        }

        ReturnToPool();
    }

    private CannonProjectile GetPooledProjectile(Vector2 position)
    {
        if (ObjectPoolManager.Instance == null ||
            projectilePrefab == null)
        {
            return null;
        }

        GameObject projectileObject =
            ObjectPoolManager.Instance.GetObject(
                projectilePrefab,
                position,
                Quaternion.identity
            );

        if (projectileObject == null)
            return null;

        CannonProjectile projectile =
            projectileObject.GetComponent<CannonProjectile>();

        if (projectile == null)
        {
            Debug.LogError(
                "Pooled Cannon projectile is missing CannonProjectile."
            );

            ObjectPoolManager.Instance.ReturnObject(projectileObject);
            return null;
        }

        return projectile;
    }

    private void ReturnToPool()
    {
        initialized = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = originalGravityScale;

        PooledObject pooledObject = GetComponent<PooledObject>();

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

    private void OnDisable()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = originalGravityScale;

        initialized = false;
    }
}
