
using System.Collections.Generic;
using UnityEngine;

public class IceFreezeEffect : MonoBehaviour
{
    private AttackableHealth health;

    private float freezeTimer;
    private IceWandProjectile shardPrefab;
    private int shardCount;
    private float shardRadius;
    private float shardDamage;

    private bool isConfigured;
    private bool subscribed;

    private readonly List<AttackableHealth> nearbyTargets = new();

    private void Awake()
    {
        health = GetComponent<AttackableHealth>();
    }

    public void Configure(
        AttackableHealth frozenTarget,
        float duration,
        IceWandProjectile newShardPrefab,
        int newShardCount,
        float newShardRadius,
        float newShardDamage)
    {
        if (health == null)
            health = GetComponent<AttackableHealth>();

        Unsubscribe();

        health = frozenTarget;
        freezeTimer = duration;

        shardPrefab = newShardPrefab;
        shardCount = newShardCount;
        shardRadius = newShardRadius;
        shardDamage = newShardDamage;

        isConfigured = true;

        if (health != null)
        {
            health.OnDied += HandleTargetDeath;
            subscribed = true;
        }

        enabled = true;
    }

    private void Update()
    {
        if (!isConfigured)
            return;

        freezeTimer -= Time.deltaTime;

        if (freezeTimer <= 0f)
            ClearEffect();
    }

    private void HandleTargetDeath()
    {
        if (!isConfigured)
            return;

        Vector3 deathPosition = transform.position;

        SpawnShards(deathPosition);
        ClearEffect();
    }

    private void SpawnShards(Vector3 spawnPosition)
    {
        if (shardPrefab == null || shardCount <= 0)
            return;

        nearbyTargets.Clear();

        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        float radiusSquared = shardRadius * shardRadius;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth candidate = targets[i];

            if (candidate == null ||
                candidate == health ||
                !candidate.gameObject.activeInHierarchy ||
                !candidate.IsAlive)
                continue;

            Vector2 offset =
                (Vector2)candidate.transform.position -
                (Vector2)spawnPosition;

            if (offset.sqrMagnitude <= radiusSquared)
                nearbyTargets.Add(candidate);
        }

        int count = Mathf.Min(shardCount, nearbyTargets.Count);

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, nearbyTargets.Count);

            AttackableHealth selectedTarget =
                nearbyTargets[randomIndex];

            nearbyTargets.RemoveAt(randomIndex);

            SpawnShard(spawnPosition, selectedTarget);
        }
    }

    private void SpawnShard(
        Vector3 spawnPosition,
        AttackableHealth target)
    {
        GameObject shardObject =
            ObjectPoolManager.Instance != null
                ? ObjectPoolManager.Instance.GetObject(
                    shardPrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    shardPrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                );

        if (shardObject == null)
            return;

        IceWandProjectile shard =
            shardObject.GetComponent<IceWandProjectile>();

        if (shard != null)
        {
            shard.InitializeShard(target, shardDamage);
        }
        else
        {
            Debug.LogWarning(
                "Ice shard prefab is missing IceWandProjectile."
            );

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(shardObject);
            else
                Destroy(shardObject);
        }
    }

    private void ClearEffect()
    {
        Unsubscribe();

        isConfigured = false;
        freezeTimer = 0f;
        shardPrefab = null;
        shardCount = 0;
        shardRadius = 0f;
        shardDamage = 0f;

        enabled = false;
    }

    private void Unsubscribe()
    {
        if (subscribed && health != null)
            health.OnDied -= HandleTargetDeath;

        subscribed = false;
    }

    private void OnDisable()
    {
        Unsubscribe();
        isConfigured = false;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
