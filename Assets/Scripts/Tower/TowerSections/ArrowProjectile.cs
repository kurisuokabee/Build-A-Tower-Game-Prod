
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
    private AttackableHealth target;
    private Transform returnPoint;
    private PooledObject pooledObject;

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

    private readonly HashSet<AttackableHealth> hitTargets = new();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(
        AttackableHealth target,
        float damage,
        bool piercing = false,
        bool returning = false,
        Transform returnPoint = null,
        float angleOffset = 0f,
        bool homing = false)
    {
        if (target == null || !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        this.target = target;
        this.damage = damage;
        this.piercing = piercing;
        returningEnabled = returning;
        this.returnPoint = returnPoint;
        homingEnabled = homing;

        lifetime = 0f;
        outboundTimer = 0f;
        homingTimer = 0f;

        initialized = true;
        isReturning = false;
        isHoming = false;

        hitTargets.Clear();

        Vector2 direction =
            ((Vector2)target.transform.position - rb.position).normalized;

        if (direction == Vector2.zero)
            direction = Vector2.up;

        direction = RotateDirection(direction, angleOffset);
        SetDirection(direction);
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
            ReturnToPool();
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
            if (!IsTargetValid(target))
                target = FindNearestTarget();

            if (isHoming && IsTargetValid(target))
                HomeTowardTarget();
        }
    }

    private bool IsTargetValid(AttackableHealth attackable)
    {
        return attackable != null &&
               attackable.isActiveAndEnabled &&
               attackable.IsAlive;
    }

    private AttackableHealth FindNearestTarget()
    {
        return AttackableTargetFinder.FindNearestTarget(rb.position);
    }

    private void HomeTowardTarget()
    {
        Vector2 direction =
            ((Vector2)target.transform.position - rb.position).normalized;

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
            ReturnToPool();
            return;
        }

        Vector2 toReturnPoint =
            (Vector2)returnPoint.position - rb.position;

        if (toReturnPoint.magnitude <= speed * Time.fixedDeltaTime)
        {
            ReturnToPool();
            return;
        }

        SetDirection(toReturnPoint.normalized);
    }

    private void BeginReturn()
    {
        isReturning = true;
        isHoming = false;
        hitTargets.Clear();
    }

    private void SetDirection(Vector2 direction)
    {
        rb.linearVelocity = direction * speed;

        float angle = Mathf.Atan2(direction.y, direction.x)
            * Mathf.Rad2Deg - 90f;

        rb.rotation = angle;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void HandleHit(Collider2D hit)
    {
        if (!initialized || hit == null)
            return;

        AttackableHealth attackable =
            hit.GetComponentInParent<AttackableHealth>();

        if (attackable != null)
        {
            if (IsTargetValid(attackable) && hitTargets.Add(attackable))
            {
                float actualDamage = isReturning
                    ? damage * returnDamageMultiplier
                    : damage;

                attackable.TakeDamage(actualDamage);
            }

            if (!piercing)
                ReturnToPool();

            return;
        }

        bool isDebris =
            hit.CompareTag("Debris") ||
            hit.transform.root.CompareTag("Debris");

        if (isDebris && !piercing)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        if (!initialized)
            return;

        initialized = false;
        target = null;
        returnPoint = null;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        hitTargets.Clear();

        if (pooledObject != null)
            pooledObject.ReturnToPool();
        else if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        else
            gameObject.SetActive(false);
    }
}
