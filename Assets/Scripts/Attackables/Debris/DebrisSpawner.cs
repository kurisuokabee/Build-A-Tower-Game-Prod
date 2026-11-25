
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{   
    public static DebrisSpawner Instance { get; private set; }

    [Header("Debris Data Types")]
    [SerializeField] private DebrisData[] debrisDatas;

    [Header("Spawn Areas")]
    [SerializeField] private SpawnArea[] spawnAreas;

    [Header("Spawn Difficulty")]
    [SerializeField, Min(0.1f)] private float startingSpawnInterval = 0.5f;
    [SerializeField, Min(0.1f)] private float minimumSpawnInterval = 0.2f;
    [SerializeField] private int levelForMaximumSpawnRate = 15;

    [Header("Debris Health Scaling")]
    [SerializeField, Min(0f)] private float healthIncreasePerLevel = 0.1f;
    [SerializeField, Min(0f)] private float healthIncreasePerSection = 0.05f;

    [Header("Initial Debris")]
    [SerializeField, Min(0)] private int initialDebrisCount = 10;
    [SerializeField] private float initialSpawnBottomY = -3f;
    [SerializeField] private float initialSpawnTopOffset = 2f;

    private readonly List<DebrisData> availableDebris = new();

    private bool isSpawningEnabled = true;
    private float spawnTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CacheAvailableDebris();
    }

    private void Start()
    {
        if (ObjectPoolManager.Instance == null)
        {
            Debug.LogError("ObjectPoolManager is missing!");
            enabled = false;
            return;
        }

        if (!HasValidSpawnAreas())
        {
            Debug.LogError("DebrisSpawner has no valid SpawnAreas!", this);
            enabled = false;
            return;
        }

        SpawnInitialDebris();
    }

    private void Update()
    {
        if (!isSpawningEnabled)
            return;

        spawnTimer += Time.deltaTime;

        float currentInterval = GetCurrentSpawnInterval();

        if (spawnTimer >= currentInterval)
        {
            spawnTimer -= currentInterval;
            SpawnDebris();
        }
    }

    private int GetPlayerLevel()
    {
        return PlayerEconomy.Instance != null
            ? PlayerEconomy.Instance.Level
            : 1;
    }

    private int GetTowerSectionCount()
    {
        return TowerBuilding.Instance != null
            ? TowerBuilding.Instance.Sections.Count
            : 0;
    }

    private float GetCurrentSpawnInterval()
    {
        int level = GetPlayerLevel();

        float progress = Mathf.InverseLerp(
            1,
            Mathf.Max(1, levelForMaximumSpawnRate),
            level
        );

        return Mathf.Lerp(
            startingSpawnInterval,
            minimumSpawnInterval,
            progress
        );
    }

    private float GetDebrisHealthMultiplier()
    {
        int level = GetPlayerLevel();
        int sections = GetTowerSectionCount();

        float levelMultiplier =
            1f + Mathf.Max(0, level - 1) * healthIncreasePerLevel;

        float sectionMultiplier =
            1f + sections * healthIncreasePerSection;

        return levelMultiplier * sectionMultiplier;
    }

    private void CacheAvailableDebris()
    {
        availableDebris.Clear();

        if (debrisDatas == null)
            return;

        foreach (DebrisData data in debrisDatas)
        {
            if (data != null && data.prefab != null)
                availableDebris.Add(data);
        }
    }

    private bool HasValidSpawnAreas()
    {
        if (spawnAreas == null)
            return false;

        foreach (SpawnArea area in spawnAreas)
        {
            if (area != null)
                return true;
        }

        return false;
    }

    private SpawnArea GetRandomSpawnArea()
    {
        int validCount = 0;

        foreach (SpawnArea area in spawnAreas)
        {
            if (area != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int selectedIndex = Random.Range(0, validCount);

        foreach (SpawnArea area in spawnAreas)
        {
            if (area == null)
                continue;

            if (selectedIndex == 0)
                return area;

            selectedIndex--;
        }

        return null;
    }

    private void SpawnInitialDebris()
    {
        for (int i = 0; i < initialDebrisCount; i++)
        {
            SpawnArea area = GetRandomSpawnArea();

            if (area == null)
                return;

            float topY =
                area.transform.position.y + initialSpawnTopOffset;

            Vector3 spawnPos = area.GetRandomPositionInRange(
                initialSpawnBottomY,
                topY
            );

            SpawnDebrisAt(spawnPos);
        }
    }

    public void RespawnInitialDebris()
    {
        SpawnInitialDebris();
    }

    private void SpawnDebris()
    {
        SpawnArea area = GetRandomSpawnArea();

        if (area == null)
            return;

        SpawnDebrisAt(area.GetRandomPosition());
    }

    private void SpawnDebrisAt(Vector3 spawnPos)
    {
        DebrisData data = ChooseDebrisData();

        if (data == null)
            return;

        GameObject debrisObj = ObjectPoolManager.Instance.GetObject(
            data.prefab,
            spawnPos,
            Quaternion.identity
        );

        if (debrisObj == null)
            return;

        DebrisSetup setup = debrisObj.GetComponent<DebrisSetup>();

        if (setup != null)
        {
            setup.Initialize(data, GetDebrisHealthMultiplier());
        }
        else
        {
            Debug.LogWarning(
                $"Debris prefab '{data.prefab.name}' is missing DebrisSetup.",
                debrisObj
            );
        }
    }

    private DebrisData ChooseDebrisData()
    {
        if (availableDebris.Count == 0)
            return null;

        float totalWeight = 0f;

        foreach (DebrisData data in availableDebris)
            totalWeight += Mathf.Max(0f, data.spawnWeight);

        if (totalWeight <= 0f)
            return availableDebris[Random.Range(0, availableDebris.Count)];

        float roll = Random.Range(0f, totalWeight);

        foreach (DebrisData data in availableDebris)
        {
            roll -= Mathf.Max(0f, data.spawnWeight);

            if (roll <= 0f)
                return data;
        }

        return availableDebris[availableDebris.Count - 1];
    }

    public void SetSpawningEnabled(bool enabled)
    {
        isSpawningEnabled = enabled;
    }

    public void ClearAllDebris()
    {
        Debris[] allDebris = FindObjectsByType<Debris>();

        foreach (var d in allDebris)
        {
            if (d != null && d.gameObject.activeInHierarchy)
            {
                PooledObject pooled = d.GetComponent<PooledObject>();

                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    ObjectPoolManager.Instance?.ReturnObject(d.gameObject);
            }
        }
    }

    public void SpawnResourceDebrisAt(Vector3 spawnPosition, float healthMultiplier)
    {
        DebrisData data = ChooseDebrisData();

        if (data == null)
            return;

        GameObject debrisObj = ObjectPoolManager.Instance != null
            ? ObjectPoolManager.Instance.GetObject(
                data.prefab,
                spawnPosition,
                Quaternion.identity
            )
            : Instantiate(data.prefab, spawnPosition, Quaternion.identity);

        if (debrisObj == null)
            return;

        DebrisSetup setup = debrisObj.GetComponent<DebrisSetup>();

        if (setup != null)
        {
            setup.Initialize(data, healthMultiplier);
        }
        else
        {
            Debug.LogWarning(
                $"Debris prefab '{data.prefab.name}' is missing DebrisSetup.",
                debrisObj
            );
        }
    }
}
