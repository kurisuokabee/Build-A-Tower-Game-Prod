
using System.Collections.Generic;
using UnityEngine;

public class ElectricWand : SakeroWand
{
    [Header("Electric Lightning")]
    [SerializeField] private ElectricLightning lightningPrefab;
    [SerializeField] private int maxTargets = 4;
    [SerializeField] private float chainRange = 3f;
    [SerializeField] private float lightningDuration = 0.15f;

    [Header("Overload - Left Upgrade")]
    [SerializeField, Range(0f, 1f)] private float stunChance = 0.2f;
    [SerializeField] private float stunDuration = 1f;

    [Header("Thunderstrike - Right Upgrade")]
    [SerializeField, Range(0f, 1f)] private float thunderstrikeChance = 0.2f;
    [SerializeField] private float thunderstrikeDamageMultiplier = 1f;
    [SerializeField] private float cameraTopOffset = 1f;
    private bool hasLeftUpgrade;
    private bool hasRightUpgrade;

    public void SetLeftUpgradeEnabled(bool enabled)
    {
        hasLeftUpgrade = enabled;
    }

    public void SetRightUpgradeEnabled(bool enabled)
    {
        hasRightUpgrade = enabled;
    }

    protected override void AttackTarget(AttackableHealth target)
    {
        if (!IsValidTarget(target) || lightningPrefab == null)
            return;

        List<AttackableHealth> hitTargets = new();

        AttackableHealth currentTarget = target;

        Vector3 previousPosition = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position;

        for (int i = 0; i < maxTargets; i++)
        {
            if (!IsValidTarget(currentTarget))
                break;

            hitTargets.Add(currentTarget);

            Vector3 targetPosition = currentTarget.transform.position;

            SpawnLightning(previousPosition, targetPosition);

            ApplyHitEffects(currentTarget, CurrentDamage);

            previousPosition = targetPosition;

            currentTarget = FindNearestTarget(
                targetPosition,
                hitTargets
            );

            if (currentTarget == null)
                break;
        }
    }

    private void ApplyHitEffects(
        AttackableHealth target,
        float damage)
    {
        if (!IsValidTarget(target))
            return;

        // Overload: chance to stun the enemy.
        if (hasLeftUpgrade && Random.value <= stunChance)
        {
            target.ApplyStun(stunDuration);
        }

        target.TakeDamage(damage);

        // Thunderstrike: additional bolt from the sky.
        if (hasRightUpgrade &&
            IsValidTarget(target) &&
            Random.value <= thunderstrikeChance)
        {
            Vector3 targetPosition = target.transform.position;

            float cameraTop = mainCamera.ViewportToWorldPoint(
                new Vector3(0.5f, 1f, 0f)
            ).y;

            Vector3 skyPosition = new Vector3(
                targetPosition.x,
                cameraTop + cameraTopOffset,
                targetPosition.z
            );

            SpawnLightning(skyPosition, targetPosition);

            target.TakeDamage(
                damage * thunderstrikeDamageMultiplier
            );
        }
    }

    private AttackableHealth FindNearestTarget(
        Vector2 origin,
        List<AttackableHealth> hitTargets)
    {
        var targets = AttackableTargetRegistry.Targets;

        AttackableHealth nearestTarget = null;
        float nearestDistanceSquared = chainRange * chainRange;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth candidate = targets[i];

            if (!IsValidTarget(candidate) ||
                hitTargets.Contains(candidate))
                continue;

            Vector2 offset =
                (Vector2)candidate.transform.position - origin;

            float distanceSquared = offset.sqrMagnitude;

            if (distanceSquared >= nearestDistanceSquared)
                continue;

            nearestDistanceSquared = distanceSquared;
            nearestTarget = candidate;
        }

        return nearestTarget;
    }

    private bool IsValidTarget(AttackableHealth target)
    {
        return target != null &&
               target.gameObject.activeInHierarchy &&
               target.IsAlive;
    }

    private void SpawnLightning(
        Vector3 startPosition,
        Vector3 endPosition)
    {
        if (lightningPrefab == null)
            return;

        GameObject lightningObject =
            ObjectPoolManager.Instance != null
                ? ObjectPoolManager.Instance.GetObject(
                    lightningPrefab.gameObject,
                    startPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    lightningPrefab.gameObject,
                    startPosition,
                    Quaternion.identity
                );

        if (lightningObject == null)
            return;

        ElectricLightning lightning =
            lightningObject.GetComponent<ElectricLightning>();

        if (lightning != null)
        {
            lightning.Initialize(
                startPosition,
                endPosition,
                lightningDuration
            );
        }
        else
        {
            Debug.LogWarning(
                "The lightning prefab is missing ElectricLightning."
            );

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(lightningObject);
            else
                Destroy(lightningObject);
        }
    }

    public void TriggerChainAttack(
        AttackableHealth target,
        float damage)
    {
        if (!IsValidTarget(target))
            return;

        List<AttackableHealth> hitTargets = new();

        AttackableHealth currentTarget = target;
        Vector3 previousPosition = target.transform.position;

        for (int i = 0; i < maxTargets; i++)
        {
            if (!IsValidTarget(currentTarget))
                break;

            hitTargets.Add(currentTarget);

            Vector3 targetPosition = currentTarget.transform.position;

            if (i == 0)
            {
                // Create a small lightning burst at the projectile impact.
                Vector2 randomOffset = Random.insideUnitCircle * 0.35f;

                Vector3 burstEnd = targetPosition +
                    new Vector3(randomOffset.x, randomOffset.y, 0f);

                SpawnLightning(targetPosition, burstEnd);
            }
            else
            {
                // Chain lightning between entities.
                SpawnLightning(previousPosition, targetPosition);
            }

            ApplyHitEffects(currentTarget, damage);

            previousPosition = targetPosition;

            currentTarget = FindNearestTarget(
                previousPosition,
                hitTargets
            );

            if (currentTarget == null)
                break;
        }
    }
    
}
