
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArrowProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float despawnTime = 3f;

    [Header("Homing")]
    [SerializeField] private float homingDelay = 0.5f;
    [SerializeField] private float homingTurnSpeed = 360f;

    [Header("Returning Arrow")]
    [SerializeField, Range(0f, 1f)]
    private float returnDamageMultiplier = 0.5f;

    [SerializeField] private float outboundDuration = 0.75f;

    private Rigidbody2D rb;
    private Enemy targetEnemy;
    private Transform returnPoint;

    private float damage;
    private float lifetime;
    private float outboundTimer;
    private float homingTimer;

    private bool initialized;
    private bool piercing;
    private bool returningEnabled;
    private bool isReturning;
    private bool homingEnabled;
    private bool isHoming;

    private readonly HashSet<EnemyHealth> hitEnemies = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(
        Enemy target,
        float damage,
        bool piercing = false,
        bool returning = false,
        Transform returnPoint = null,
        float angleOffset = 0f,
        bool homing = false)
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        targetEnemy = target;
        this.damage = damage;
        this.piercing = piercing;
        returningEnabled = returning;
        this.returnPoint = returnPoint;
        homingEnabled = homing;

        Vector2 direction =
            ((Vector2)target.transform.position - rb.position).normalized;

        if (direction == Vector2.zero)
            direction = Vector2.up;

        direction = RotateDirection(direction, angleOffset);

        SetDirection(direction);
        initialized = true;
    }

    private Vector2 RotateDirection(Vector2 direction, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    private void Update()
    {
        if (!initialized)
            return;

        lifetime += Time.deltaTime;

        if (lifetime >= despawnTime)
        {
            Destroy(gameObject);
            return;
        }

        if (homingEnabled && !isReturning)
        {
            homingTimer += Time.deltaTime;

            if (homingTimer >= homingDelay)
                isHoming = true;
        }
    }

    private void FixedUpdate()
    {
        if (!initialized)
            return;

        if (returningEnabled && !isReturning)
        {
            outboundTimer += Time.fixedDeltaTime;

            if (outboundTimer >= outboundDuration)
                BeginReturn();
        }

        if (isReturning)
        {
            MoveTowardReturnPoint();
            return;
        }

        if (homingEnabled)
        {
            if (!IsTargetValid(targetEnemy))
                targetEnemy = FindNearestActiveEnemy();

            if (isHoming && IsTargetValid(targetEnemy))
                HomeTowardTarget();
        }
    }

    private bool IsTargetValid(Enemy target)
    {
        return target != null &&
               target.gameObject.activeInHierarchy;
    }

    private Enemy FindNearestActiveEnemy()
    {
        Enemy[] enemies =
            FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (!IsTargetValid(enemy))
                continue;

            float distance = (
                (Vector2)enemy.transform.position - rb.position
            ).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    private void HomeTowardTarget()
    {
        Vector2 direction =
            ((Vector2)targetEnemy.transform.position - rb.position).normalized;

        if (direction == Vector2.zero)
            return;

        Vector2 currentDirection = rb.linearVelocity.normalized;

        Vector2 newDirection = Vector2.MoveTowards(
            currentDirection,
            direction,
            homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime
        ).normalized;

        SetDirection(newDirection);
    }

    private void MoveTowardReturnPoint()
    {
        if (returnPoint == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector2 toReturnPoint =
            (Vector2)returnPoint.position - rb.position;

        if (toReturnPoint.magnitude <= speed * Time.fixedDeltaTime)
        {
            Destroy(gameObject);
            return;
        }

        SetDirection(toReturnPoint.normalized);
    }

    private void BeginReturn()
    {
        isReturning = true;
        isHoming = false;

        // Allow the returning arrow to hit enemies again.
        hitEnemies.Clear();
    }

    private void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg - 90f;

        rb.rotation = angle;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider2D hit)
    {
        if (!initialized || hit == null)
            return;

        EnemyHealth enemyHealth =
            hit.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            if (hitEnemies.Add(enemyHealth))
            {
                float actualDamage = isReturning
                    ? damage * returnDamageMultiplier
                    : damage;

                enemyHealth.TakeDamage(actualDamage);
            }

            if (!piercing)
                Destroy(gameObject);

            return;
        }

        bool isDebris =
            hit.CompareTag("Debris") ||
            hit.transform.root.CompareTag("Debris");

        if (isDebris && !piercing)
            Destroy(gameObject);
    }
}
