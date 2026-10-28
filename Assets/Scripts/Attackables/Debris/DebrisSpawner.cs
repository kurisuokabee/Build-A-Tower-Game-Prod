
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
    [Header("Debris Data Types")]
    [SerializeField] private DebrisData[] debrisDatas;

    [Header("Spawn Areas")]
    [SerializeField] private SpawnArea[] spawnAreas;

    [Header("Spawn Settings")]
    [SerializeField, Min(0.1f)] private float spawnInterval = 1.5f;

    [Header("Initial Debris")]
    [SerializeField, Min(0)] private int initialDebrisCount = 10;
    [SerializeField] private float initialSpawnBottomY = -3f;
    [SerializeField] private float initialSpawnTopOffset = 2f;

    [Header("Size Spawn Weights")]
    [SerializeField] private float smallChance = 70f;
    [SerializeField] private float mediumChance = 25f;
    [SerializeField] private float largeChance = 5f;

    private readonly List<DebrisData> availableDebris = new();

    private float spawnTimer;

    private void Awake()
    {
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
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer -= spawnInterval;
            SpawnDebris();
        }
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
            setup.Initialize(data);
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

        float totalWeight =
            Mathf.Max(0f, smallChance) +
            Mathf.Max(0f, mediumChance) +
            Mathf.Max(0f, largeChance);

        if (totalWeight <= 0f)
        {
            return availableDebris[
                Random.Range(0, availableDebris.Count)
            ];
        }

        float roll = Random.value * totalWeight;

        DebrisData.DebrisSize targetSize;

        if (roll < Mathf.Max(0f, smallChance))
        {
            targetSize = DebrisData.DebrisSize.Small;
        }
        else if (roll <
                 Mathf.Max(0f, smallChance) +
                 Mathf.Max(0f, mediumChance))
        {
            targetSize = DebrisData.DebrisSize.Medium;
        }
        else
        {
            targetSize = DebrisData.DebrisSize.Large;
        }

        DebrisData selected = null;
        int matchCount = 0;

        foreach (DebrisData data in availableDebris)
        {
            if (data.size != targetSize)
                continue;

            matchCount++;

            if (Random.Range(0, matchCount) == 0)
                selected = data;
        }

        return selected != null
            ? selected
            : availableDebris[Random.Range(0, availableDebris.Count)];
    }
}
