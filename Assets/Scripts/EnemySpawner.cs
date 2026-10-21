
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData[] enemies;

    [Header("Tier Spawn Chances")]
    [SerializeField, Min(0)] private float tier1Chance = 70f;
    [SerializeField, Min(0)] private float tier2Chance = 25f;
    [SerializeField, Min(0)] private float tier3Chance = 5f;

    [Header("Spawn Areas")]
    [SerializeField] private SpawnArea[] spawnAreas;

    [Header("Tower")]
    [SerializeField] private TowerBuilding tower;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.01f)] private float spawnInterval = 2f;

    private float spawnTimer;

    private void Update()
    {   
        if (tower == null || tower.Sections.Count == 0)
        return;
        
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer -= spawnInterval;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (spawnAreas == null || spawnAreas.Length == 0)
            return;

        EnemyData data = ChooseEnemy();

        if (data == null || data.prefab == null)
            return;

        SpawnArea area =
            spawnAreas[Random.Range(0, spawnAreas.Length)];

        if (area == null)
            return;

        GameObject enemy = Instantiate(
            data.prefab,
            area.GetRandomPosition(),
            Quaternion.identity
        );

        TowerSection target = FindTarget(
            data.movementType,
            enemy.transform.position
        );

        EnemySetup setup = enemy.GetComponent<EnemySetup>();
        if (setup != null)
        {
            setup.Initialize(
                data.movementType, 
                data.attackType, 
                target, 
                data.sprite);
        }

    }

    private EnemyData ChooseEnemy()
    {
        List<EnemyData> availableEnemies = new List<EnemyData>();

        foreach (EnemyData enemy in enemies)
        {
            if (enemy != null && enemy.prefab != null &&
                GetTierChance(enemy.tier) > 0f)
            {
                availableEnemies.Add(enemy);
            }
        }

        if (availableEnemies.Count == 0)
            return null;

        // Find which tiers actually have available enemies.
        bool hasTier1 = false;
        bool hasTier2 = false;
        bool hasTier3 = false;

        foreach (EnemyData enemy in availableEnemies)
        {
            if (enemy.tier == 1) hasTier1 = true;
            if (enemy.tier == 2) hasTier2 = true;
            if (enemy.tier == 3) hasTier3 = true;
        }

        float totalChance = 0f;

        if (hasTier1) totalChance += tier1Chance;
        if (hasTier2) totalChance += tier2Chance;
        if (hasTier3) totalChance += tier3Chance;

        if (totalChance <= 0f)
            return null;

        float roll = Random.value * totalChance;
        int selectedTier;

        if (hasTier1 && roll < tier1Chance)
        {
            selectedTier = 1;
        }
        else
        {
            if (hasTier1) roll -= tier1Chance;

            if (hasTier2 && roll < tier2Chance)
            {
                selectedTier = 2;
            }
            else
            {
                selectedTier = 3;
            }
        }

        List<EnemyData> tierEnemies = new List<EnemyData>();

        foreach (EnemyData enemy in availableEnemies)
        {
            if (enemy.tier == selectedTier)
                tierEnemies.Add(enemy);
        }

        // Handle any edge case where a tier has no valid enemies.
        if (tierEnemies.Count == 0)
            return availableEnemies[Random.Range(0, availableEnemies.Count)];

        return tierEnemies[Random.Range(0, tierEnemies.Count)];
    }

    private float GetTierChance(int tier)
    {
        switch (tier)
        {
            case 1: return tier1Chance;
            case 2: return tier2Chance;
            case 3: return tier3Chance;
            default: return 0f;
        }
    }

    private TowerSection FindTarget(
        EnemyMovement.MovementType movementType,
        Vector3 enemyPosition)
    {
        if (tower == null || tower.Sections.Count == 0)
            return null;

        if (movementType == EnemyMovement.MovementType.Land)
        {
            TowerSection bottom = tower.Sections[0];

            if (bottom != null && bottom.gameObject.activeInHierarchy)
                return bottom;

            return null;
        }

        TowerSection closest = null;
        float closestDistance = float.MaxValue;

        foreach (TowerSection section in tower.Sections)
        {
            if (section == null || !section.gameObject.activeInHierarchy)
                continue;

            float distance =
                (section.transform.position - enemyPosition).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = section;
            }
        }

        return closest;
    }
}
