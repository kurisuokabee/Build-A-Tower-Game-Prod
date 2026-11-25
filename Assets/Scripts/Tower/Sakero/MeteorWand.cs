using System.Collections.Generic;
using UnityEngine;

public class MeteorWand : SakeroWand
{
    [Header("Meteor")]
    [SerializeField] private MeteorProjectile meteorPrefab;

    [Header("Additional Targets")]
    [SerializeField] private int additionalTargetCount = 2;
    [SerializeField] private float additionalTargetRadius = 4f;

    [Header("Resource Debris Upgrade")]
    [SerializeField, Range(0f, 1f)] private float resourceDebrisChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float resourceDebrisHealthMultiplier = 0.5f;

    [Header("Meteor Spawn")]
    [SerializeField] private float spawnHeight = 1.1f;
    [SerializeField, Range(0f, 0.5f)]
    private float horizontalSpawnSpread = 0.25f;

    private bool hasLeftUpgrade;
    private bool hasRightUpgrade;

    private readonly List<AttackableHealth> nearbyTargets = new();

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
        if (target == null || !target.IsAlive)
            return;

        SpawnMeteor(target);

        if (!hasRightUpgrade)
            return;

        FindAdditionalTargets(target);

        foreach (AttackableHealth additionalTarget in nearbyTargets)
        {
            SpawnMeteor(additionalTarget);
        }
    }

    private void FindAdditionalTargets(AttackableHealth mainTarget)
    {
        nearbyTargets.Clear();

        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        List<AttackableHealth> candidates = new();

        float radiusSquared =
            additionalTargetRadius * additionalTargetRadius;

        Vector2 mainPosition = mainTarget.transform.position;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth candidate = targets[i];

            if (candidate == null ||
                candidate == mainTarget ||
                !candidate.gameObject.activeInHierarchy ||
                !candidate.IsAlive)
                continue;

            Vector2 offset =
                (Vector2)candidate.transform.position - mainPosition;

            if (offset.sqrMagnitude <= radiusSquared)
                candidates.Add(candidate);
        }

        int count = Mathf.Min(
            additionalTargetCount,
            candidates.Count
        );

        for (int i = 0; i < count; i++)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            nearbyTargets.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }
    }

    private void SpawnMeteor(AttackableHealth target)
    {
        if (meteorPrefab == null || target == null)
            return;

        Vector3 spawnPosition = GetRandomSpawnPosition(target);

        GameObject meteorObject =
            ObjectPoolManager.Instance != null
                ? ObjectPoolManager.Instance.GetObject(
                    meteorPrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                )
                : Instantiate(
                    meteorPrefab.gameObject,
                    spawnPosition,
                    Quaternion.identity
                );

        if (meteorObject == null)
            return;

        MeteorProjectile meteor =
            meteorObject.GetComponent<MeteorProjectile>();

        if (meteor != null)
        {
            meteor.Initialize(
                target,
                CurrentDamage,
                hasLeftUpgrade,
                resourceDebrisChance,
                resourceDebrisHealthMultiplier
            );
        }
        else
        {
            Debug.LogWarning(
                "Meteor prefab is missing MeteorProjectile."
            );

            if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(meteorObject);
            else
                Destroy(meteorObject);
        }
    }

    private Vector3 GetRandomSpawnPosition(AttackableHealth target)
    {
        Vector3 targetViewport =
            mainCamera.WorldToViewportPoint(target.transform.position);

        float randomOffset = Random.Range(
            -horizontalSpawnSpread,
            horizontalSpawnSpread
        );

        float viewportX = Mathf.Clamp01(
            targetViewport.x + randomOffset
        );

        float cameraDistance = Mathf.Abs(
            mainCamera.transform.position.z - target.transform.position.z
        );

        Vector3 spawnPosition = mainCamera.ViewportToWorldPoint(
            new Vector3(
                viewportX,
                spawnHeight,
                cameraDistance
            )
        );

        spawnPosition.z = target.transform.position.z;

        return spawnPosition;
    }

    public void CreateImpactMeteor(AttackableHealth target)
    {
        if (target == null || !target.IsAlive)
            return;

        SpawnMeteor(target);
    }
}