
using System.Collections.Generic;
using UnityEngine;

public class VortexEffect : MonoBehaviour
{
    [Header("Vortex Settings")]
    [SerializeField] private float radius = 3f;
    [SerializeField] private float pullSpeed = 8f;
    [SerializeField] private float duration = 3f;

    [Header("Soul Harvest - Left Upgrade")]
    [SerializeField, Min(1f)] private float rewardMultiplier = 2f;

    [Header("Gravitational Crush - Right Upgrade")]
    [SerializeField, Min(0f)] private float damagePerAdditionalEnemy = 0.25f;

    private float damage;
    private float damageInterval;

    private float damageTimer;
    private float lifeTimer;

    private bool isPersistent;
    private bool isPullingOnce;

    private bool soulHarvestEnabled;
    private bool gravitationalCrushEnabled;
    private bool returnAfterDuration;
    private readonly List<AttackableHealth> pulledTargets = new();
    private readonly List<AttackableHealth> nearbyTargets = new();

    private void OnEnable()
    {
        lifeTimer = 0f;
        damageTimer = 0f;

        isPersistent = false;
        isPullingOnce = false;

        soulHarvestEnabled = false;
        gravitationalCrushEnabled = false;

        returnAfterDuration = false;

        pulledTargets.Clear();
        nearbyTargets.Clear();
    }

    public void SetDamageStats(float newDamage, float attacksPerSecond)
    {
        damage = Mathf.Max(0f, newDamage);
        damageInterval = 1f / Mathf.Max(0.01f, attacksPerSecond);
    }

    public void SetPersistent(bool persistent)
    {
        isPersistent = persistent;
    }

    public void SetUpgrades(
        bool hasSoulHarvest,
        bool hasGravitationalCrush)
    {
        soulHarvestEnabled = hasSoulHarvest;
        gravitationalCrushEnabled = hasGravitationalCrush;
    }

    private void Update()
    {
        lifeTimer += Time.deltaTime;

        if (isPersistent)
        {
            PullTargets();

            damageTimer += Time.deltaTime;

            if (damageTimer >= damageInterval)
            {
                damageTimer -= damageInterval;
                DamageTargets();
            }
        }
        else
        {
            PullCapturedTargets();
        }

        if ((!isPersistent || returnAfterDuration) &&
            lifeTimer >= duration)
        {
            ReturnToPool();
}
    }

    private void DamageTargets()
    {
        nearbyTargets.Clear();

        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        float radiusSquared = radius * radius;
        Vector2 vortexPosition = transform.position;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (!IsValidTarget(target))
                continue;

            Vector2 offset =
                (Vector2)target.transform.position - vortexPosition;

            if (offset.sqrMagnitude <= radiusSquared)
                nearbyTargets.Add(target);
        }

        int targetCount = nearbyTargets.Count;

        float damageMultiplier = gravitationalCrushEnabled
            ? 1f + Mathf.Max(0, targetCount - 1) * damagePerAdditionalEnemy
            : 1f;

        float finalDamage = damage * damageMultiplier;

        foreach (AttackableHealth target in nearbyTargets)
        {
            if (!IsValidTarget(target))
                continue;

            bool willDieFromHit = target.CurrentHealth <= finalDamage;

            // Set the reward multiplier before damage is applied.
            if (soulHarvestEnabled && willDieFromHit)
            {
                AttackableReward reward =
                    target.GetComponent<AttackableReward>();

                if (reward != null)
                    reward.SetNextRewardMultiplier(rewardMultiplier);
            }

            target.TakeDamage(finalDamage);
        }
    }

    private void PullTargets()
    {
        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        float radiusSquared = radius * radius;
        Vector2 vortexPosition = transform.position;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (!IsValidTarget(target))
                continue;

            Vector2 targetPosition = target.transform.position;
            Vector2 offset = vortexPosition - targetPosition;

            if (offset.sqrMagnitude > radiusSquared)
                continue;

            target.transform.position = Vector2.MoveTowards(
                targetPosition,
                vortexPosition,
                pullSpeed * Time.deltaTime
            );
        }
    }

    public void PullTargetsOnce()
    {
        pulledTargets.Clear();

        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        float radiusSquared = radius * radius;
        Vector2 vortexPosition = transform.position;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (!IsValidTarget(target))
                continue;

            Vector2 offset =
                vortexPosition - (Vector2)target.transform.position;

            if (offset.sqrMagnitude > radiusSquared)
                continue;

            pulledTargets.Add(target);
        }

        isPullingOnce = pulledTargets.Count > 0;
    }

    private void PullCapturedTargets()
    {
        if (!isPullingOnce)
            return;

        Vector2 vortexPosition = transform.position;
        bool allArrived = true;

        for (int i = pulledTargets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = pulledTargets[i];

            if (!IsValidTarget(target))
            {
                pulledTargets.RemoveAt(i);
                continue;
            }

            Vector2 targetPosition = target.transform.position;

            if ((targetPosition - vortexPosition).sqrMagnitude <= 0.01f)
                continue;

            allArrived = false;

            target.transform.position = Vector2.MoveTowards(
                targetPosition,
                vortexPosition,
                pullSpeed * Time.deltaTime
            );
        }

        if (allArrived || pulledTargets.Count == 0)
            isPullingOnce = false;
    }
    public void SetTemporaryPersistent(float lifetime)
    {
        isPersistent = true;
        returnAfterDuration = true;
        duration = Mathf.Max(0.1f, lifetime);
    }

    private bool IsValidTarget(AttackableHealth target)
    {
        return target != null &&
               target.gameObject.activeInHierarchy &&
               target.IsAlive;
    }

    private void ReturnToPool()
    {
        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        else
            gameObject.SetActive(false);
    }
}
