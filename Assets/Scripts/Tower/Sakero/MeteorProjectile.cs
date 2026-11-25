using UnityEngine;

public class MeteorProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float hitDistance = 0.2f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionPrefab;
    private AttackableHealth target;
    private float damage;

    private DebrisSpawner debrisSpawner;
    private bool canSpawnResourceDebris;
    private float resourceDebrisChance;
    private float resourceDebrisHealthMultiplier;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(
        AttackableHealth newTarget,
        float newDamage,
        bool spawnResourceDebris,
        float debrisChance,
        float debrisHealthMultiplier)
    {
        target = newTarget;
        damage = newDamage;

        canSpawnResourceDebris = spawnResourceDebris;
        resourceDebrisChance = debrisChance;
        resourceDebrisHealthMultiplier = debrisHealthMultiplier;
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
    
        if (direction.sqrMagnitude <= hitDistance * hitDistance)
        {
            Explode(targetPosition);
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );

        if (direction.sqrMagnitude > 0f)
        {
            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(
                0f,
                0f,
                angle
            );
        }
    }
    

    
    private void Explode(Vector3 explosionPosition)
    {
        var targets = AttackableTargetRegistry.Targets;

        if (targets != null)
        {
            float radiusSquared = explosionRadius * explosionRadius;

            for (int i = targets.Count - 1; i >= 0; i--)
            {
                AttackableHealth hitTarget = targets[i];

                if (hitTarget == null ||
                    !hitTarget.gameObject.activeInHierarchy ||
                    !hitTarget.IsAlive)
                    continue;

                Vector2 offset =
                    (Vector2)hitTarget.transform.position -
                    (Vector2)explosionPosition;

                if (offset.sqrMagnitude > radiusSquared)
                    continue;

                hitTarget.TakeDamage(damage);
            }
        }

        // Spawn only one explosion per meteor.
        if (ObjectPoolManager.Instance != null && explosionPrefab != null)
        {
            GameObject explosion = ObjectPoolManager.Instance.GetObject(
                explosionPrefab,
                explosionPosition,
                Quaternion.identity
            );

            if (explosion != null)
            {
                ExplosionEffect effect = explosion.GetComponent<ExplosionEffect>();

                if (effect != null)
                    effect.Play(explosionRadius);
                else
                    ObjectPoolManager.Instance.ReturnObject(explosion);
            }
        }

        if (canSpawnResourceDebris &&
            DebrisSpawner.Instance != null &&
            Random.value <= resourceDebrisChance)
        {
            DebrisSpawner.Instance.SpawnResourceDebrisAt(
                explosionPosition,
                resourceDebrisHealthMultiplier
            );
        }

        ReturnToPool();
    }


    private void ReturnToPool()
    {
        target = null;
        damage = 0f;
        canSpawnResourceDebris = false;

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