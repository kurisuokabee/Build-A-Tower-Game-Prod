
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowFiringController : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private ArrowProjectile arrowPrefab;
    [SerializeField] private int projectileCount = 1;

    [Header("Arrow Spread")]
    [SerializeField] private float angleGap = 15f;

    [Header("Barrage Arrow")]
    [SerializeField] private float barrageShotDelay = 0.15f;

    [Header("Extra Arrow Upgrades")]
    [SerializeField] private int extraArrowsPerUpgrade = 2;

    [Header("Arrow Damage Penalties")]
    [SerializeField, Range(0f, 1f)]
    private float extraArrowDamageMultiplier = 0.5f;

    [SerializeField, Range(0f, 1f)]
    private float barrageDamageMultiplier = 0.5f;

    private TowerSection towerSection;

    private void Awake()
    {
        towerSection = GetComponent<TowerSection>();
    }

    public void Fire(
        AttackableHealth target,
        bool piercing,
        bool returning,
        bool homing,
        bool barrage,
        float damage)
    {
        Fire(
            target,
            piercing,
            returning,
            homing,
            barrage,
            false,
            damage
        );
    }

    public void Fire(
        AttackableHealth target,
        bool piercing,
        bool returning,
        bool homing,
        bool barrage,
        bool extraArrows,
        float damage)
    {
        if (firePoint == null ||
            arrowPrefab == null ||
            target == null ||
            !target.IsAlive ||
            towerSection == null)
        {
            return;
        }

        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager is missing from the scene.");
            return;
        }

        int arrowCount = projectileCount;

        if (extraArrows)
            arrowCount += extraArrowsPerUpgrade;

        if (barrage)
            arrowCount += extraArrowsPerUpgrade;

        float damagePerArrow = damage;

        if (arrowCount > projectileCount)
            damagePerArrow *= extraArrowDamageMultiplier;

        Vector2 direction =
            ((Vector2)target.transform.position -
             (Vector2)firePoint.position).normalized;

        if (direction == Vector2.zero)
            direction = Vector2.up;

        if (barrage)
        {
            damagePerArrow *= barrageDamageMultiplier;

            StartCoroutine(
                FireBarrage(
                    target,
                    arrowCount,
                    damagePerArrow,
                    direction,
                    piercing,
                    returning,
                    homing
                )
            );
        }
        else
        {
            FireSpreadArrows(
                target,
                arrowCount,
                damagePerArrow,
                direction,
                piercing,
                returning,
                homing
            );
        }
    }

    private IEnumerator FireBarrage(
        AttackableHealth target,
        int totalArrows,
        float damage,
        Vector2 direction,
        bool piercing,
        bool returning,
        bool homing)
    {
        for (int i = 0; i < totalArrows; i++)
        {
            FireArrow(
                target,
                damage,
                direction,
                piercing,
                returning,
                homing
            );

            if (i < totalArrows - 1)
                yield return new WaitForSeconds(barrageShotDelay);
        }
    }

    private void FireSpreadArrows(
        AttackableHealth target,
        int count,
        float damage,
        Vector2 baseDirection,
        bool piercing,
        bool returning,
        bool homing)
    {
        for (int i = 0; i < count; i++)
        {
            float angleOffset =
                (i - (count - 1) / 2f) * angleGap;

            Vector2 direction =
                RotateDirection(baseDirection, angleOffset);

            FireArrow(
                target,
                damage,
                direction,
                piercing,
                returning,
                homing
            );
        }
    }

    private Vector2 RotateDirection(
        Vector2 direction,
        float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos
        ).normalized;
    }

    private void FireArrow(
        AttackableHealth target,
        float damage,
        Vector2 direction,
        bool piercing,
        bool returning,
        bool homing)
    {
        GameObject arrowObject =
            ObjectPoolManager.Instance.GetObject(
                arrowPrefab.gameObject,
                firePoint.position,
                Quaternion.identity
            );

        if (arrowObject == null)
            return;

        ArrowProjectile projectile =
            arrowObject.GetComponent<ArrowProjectile>();

        if (projectile == null)
        {
            Debug.LogError("Arrow prefab is missing ArrowProjectile.");
            ObjectPoolManager.Instance.ReturnObject(arrowObject);
            return;
        }

        projectile.Initialize(
            target,
            damage,
            piercing,
            returning,
            firePoint,
            direction,
            homing,
            towerSection
        );
    }

    public void FireHoming(
        IReadOnlyList<AttackableHealth> targets,
        bool extraArrows,
        float damage)
    {
        if (firePoint == null ||
            arrowPrefab == null ||
            targets == null ||
            targets.Count == 0 ||
            towerSection == null)
        {
            return;
        }

        int arrowCount = projectileCount;

        if (extraArrows)
            arrowCount += extraArrowsPerUpgrade;

        int arrowsToFire = Mathf.Min(
            arrowCount,
            targets.Count
        );

        float damagePerArrow = damage;

        if (arrowCount > projectileCount)
            damagePerArrow *= extraArrowDamageMultiplier;

        for (int i = 0; i < arrowsToFire; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null || !target.IsAlive)
                continue;

            float randomAngle = Random.Range(0f, 360f);

            Vector2 randomDirection = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );

            FireArrow(
                target,
                damagePerArrow,
                randomDirection,
                false,
                false,
                true
            );
        }
    }
}
