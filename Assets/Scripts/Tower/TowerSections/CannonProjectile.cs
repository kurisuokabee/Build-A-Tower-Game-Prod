
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
    private Vector2 targetPosition;
    private float damage;
    private float lifetime;
    private bool initialized;
    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Enemy target, float damage, float aoe)
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        targetPosition = target.transform.position;
        this.damage = damage;
        areaOfEffect = aoe;

        Vector2 startPosition = rb.position;
        Vector2 direction = targetPosition - startPosition;

        float flightTime = Mathf.Max(
            Mathf.Abs(direction.x) / launchSpeed,
            0.2f
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
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!initialized || hasExploded)
            return;

        if (collision.collider.GetComponentInParent<EnemyHealth>() != null ||
            collision.collider.CompareTag("Debris") ||
            collision.collider.transform.root.CompareTag("Debris"))
        {
            Explode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!initialized || hasExploded)
            return;

        if (other.GetComponentInParent<EnemyHealth>() != null ||
            other.CompareTag("Debris") ||
            other.transform.root.CompareTag("Debris"))
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (hasExploded)
            return;

        hasExploded = true;

        DamageAllInRange(
            transform.position,
            areaOfEffect,
            damage
        );

        Destroy(gameObject);
    }

    private void DamageAllInRange(
        Vector2 center,
        float radius,
        float damageAmount)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        float radiusSquared = radius * radius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 offset = (Vector2)enemy.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
            {
                enemy.TakeDamage(damageAmount);
            }
        }
    }
}
