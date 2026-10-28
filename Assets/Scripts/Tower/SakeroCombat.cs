
using UnityEngine;

public class SakeroCombat : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float attacksPerSecond = 2f;

    [Header("Projectile")]
    [SerializeField] private SakeroMagicProjectile projectilePrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private float projectileSpawnOffset = 0.3f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Serenity Settings")]
    [SerializeField] private float serenityRange = 5f;

    private float tickTimer;
    private Camera mainCamera;

    public float CurrentDamage { get; private set; }
    public float CurrentAttacksPerSecond { get; private set; }

    private void Awake()
    {
        CurrentDamage = baseDamage;
        CurrentAttacksPerSecond = attacksPerSecond;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null ||
            CurrentAttacksPerSecond <= 0f)
        {
            return;
        }

        // Serenity automatically targets everything.
        if (TowerUtilities.Instance != null &&
            TowerUtilities.Instance.IsSerenityActive)
        {
            tickTimer += Time.deltaTime;

            float serenityInterval = 1f / CurrentAttacksPerSecond;

            while (tickTimer >= serenityInterval)
            {
                tickTimer -= serenityInterval;
                AttackAllTargets();
            }

            return;
        }

        Vector2 mouseWorldPosition = GetMouseWorldPosition();

        AttackableHealth target = GetHoveredEnemy(mouseWorldPosition);

        if (target == null)
        {
            tickTimer = 0f;
            return;
        }

        tickTimer += Time.deltaTime;

        float interval = 1f / CurrentAttacksPerSecond;

        if (tickTimer >= interval)
        {
            tickTimer -= interval;
            ShootProjectile(target);
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;

        return mainCamera.ScreenToWorldPoint(mousePosition);
    }

    private AttackableHealth GetHoveredEnemy(Vector2 mousePosition)
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
            {
                continue;
            }

            Vector2 offset =
                (Vector2)target.transform.position - origin;

            if (offset.sqrMagnitude > rangeSquared)
                continue;

            ShootProjectile(target);
        }
    }
    
    private void ShootProjectile(AttackableHealth target)
    {
        if (projectilePrefab == null || target == null)
            return;

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

        SakeroMagicProjectile projectile =
            projectileObject.GetComponent<SakeroMagicProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(target, CurrentDamage);
        }
        else
        {
            Debug.LogWarning(
                "The projectile prefab is missing SakeroMagicProjectile."
            );

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(projectileObject);
            else
                Destroy(projectileObject);
        }
    }

    public void ApplyUpgrade(
        float damageIncrease,
        float attackSpeedIncrease)
    {
        CurrentDamage += damageIncrease;
        CurrentAttacksPerSecond += attackSpeedIncrease;
    }
}
