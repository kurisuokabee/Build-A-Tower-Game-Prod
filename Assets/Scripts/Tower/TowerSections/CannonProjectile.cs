
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CannonProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float despawnTime = 4f;

    [Header("Explosion")]
    [SerializeField] private float areaOfEffect = 2f;

    private Rigidbody2D rb;
    private float damage;
    private float lifetime;
    private bool initialized;
    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(AttackableHealth target, float damage, float aoe)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        lifetime = 0f;
        initialized = false;
        hasExploded = false;

        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        this.damage = damage;
        areaOfEffect = aoe;

        Vector2 startPosition = rb.position;
        Vector2 targetPosition = target.transform.position;
        Vector2 direction = targetPosition - startPosition;

        float flightTime = Mathf.Max(
            direction.magnitude / Mathf.Max(launchSpeed, 0.01f),
            0.5f
        );

        float gravity = Physics2D.gravity.y * rb.gravityScale;

        Vector2 velocity = new Vector2(
            direction.x / flightTime,
            (direction.y - 0.5f * gravity * flightTime * flightTime)
                / flightTime
        );

        rb.linearVelocity = velocity;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized || hasExploded)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= despawnTime)
            ReturnToPool();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hasExploded)
            return;

        if (IsValidTarget(other))
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

        DamageAllInRange(transform.position, areaOfEffect, damage);

        ReturnToPool();
    }

    private void DamageAllInRange(
        Vector2 center,
        float radius,
        float damageAmount)
    {
        var targets = AttackableTargetRegistry.Targets;
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

            Vector2 offset = (Vector2)target.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
                target.TakeDamage(damageAmount);
        }
    }

    private void ReturnToPool()
    {
        initialized = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

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
}
