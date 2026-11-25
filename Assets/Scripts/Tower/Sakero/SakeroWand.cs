
using System;
using UnityEngine;

public abstract class SakeroWand : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float attacksPerSecond = 2f;

    [Header("Targeting")]
    [SerializeField] protected LayerMask enemyLayer;

    [Header("Projectile")]
    [SerializeField] protected Transform muzzlePoint;
    [SerializeField] protected float projectileSpawnOffset = 0.3f;

    [Header("Serenity Settings")]
    [SerializeField] public float serenityRange = 5f;

    protected Camera mainCamera;

    private float tickTimer;

    public float CurrentDamage { get; private set; }
    public float CurrentAttacksPerSecond { get; private set; }

    protected virtual void Awake()
    {
        CurrentDamage = baseDamage;
        CurrentAttacksPerSecond = attacksPerSecond;
        mainCamera = Camera.main;
    }

    protected virtual void Update()
    {
        if (mainCamera == null ||
            CurrentAttacksPerSecond <= 0f)
            return;

        bool serenityActive =
            TowerUtilities.Instance != null &&
            TowerUtilities.Instance.IsSerenityActive;

        if (serenityActive)
        {
            tickTimer += Time.deltaTime;

            float interval = 1f / CurrentAttacksPerSecond;

            while (tickTimer >= interval)
            {
                tickTimer -= interval;
                AttackAllTargets();
            }

            return;
        }

        AttackableHealth target =
            GetHoveredEnemy(GetMouseWorldPosition());

        if (target == null)
        {
            tickTimer = 0f;
            return;
        }

        tickTimer += Time.deltaTime;

        float attackInterval = 1f / CurrentAttacksPerSecond;

        if (tickTimer >= attackInterval)
        {
            tickTimer -= attackInterval;
            AttackTarget(target);
        }
    }

    protected Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;

        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    protected AttackableHealth GetHoveredEnemy(Vector2 mousePosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(
            mousePosition,
            enemyLayer
        );

        if (hit == null)
            return null;

        AttackableHealth target =
            hit.GetComponentInParent<AttackableHealth>();

        if (target == null || !target.IsAlive)
            return null;

        return target;
    }

    private void AttackAllTargets()
    {
        var targets = AttackableTargetRegistry.Targets;

        float rangeSquared = serenityRange * serenityRange;
        Vector2 origin = transform.position;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                !target.IsAlive)
                continue;

            Vector2 offset =
                (Vector2)target.transform.position - origin;

            if (offset.sqrMagnitude > rangeSquared)
                continue;

            AttackTarget(target);
        }
    }

    protected abstract void AttackTarget(AttackableHealth target);

    protected Vector3 GetProjectileSpawnPosition(
        AttackableHealth target)
    {
        Vector3 spawnPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position;

        Vector2 direction =
            (Vector2)(target.transform.position - spawnPosition);

        if (direction.sqrMagnitude > 0f)
        {
            spawnPosition +=
                (Vector3)(direction.normalized * projectileSpawnOffset);
        }

        return spawnPosition;
    }

    public void ApplyUpgrade(
        float damageIncrease,
        float attackSpeedIncrease)
    {
        CurrentDamage += damageIncrease;
        CurrentAttacksPerSecond += attackSpeedIncrease;
    }

    protected virtual void OnDisable()
    {
        
    }
}
