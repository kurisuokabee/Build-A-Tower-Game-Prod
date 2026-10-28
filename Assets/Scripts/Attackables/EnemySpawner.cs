
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData[] enemies;

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
        {
            spawnTimer = 0f;
            return;
        }

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

        SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Length)];

        if (area == null)
            return;

        GameObject enemy = Instantiate(
            data.prefab,
            area.GetRandomPosition(),
            Quaternion.identity
        );

        TowerSection target = FindTarget(enemy.transform.position);

        EnemySetup setup = enemy.GetComponent<EnemySetup>();

        if (setup != null)
        {
            setup.Initialize(
                data.attackType,
                target,
                data.sprite
            );
        }
    }

    private EnemyData ChooseEnemy()
    {
        if (enemies == null || enemies.Length == 0)
            return null;

        return enemies[Random.Range(0, enemies.Length)];
    }

    private TowerSection FindTarget(Vector3 enemyPosition)
    {
        if (tower == null || tower.Sections.Count == 0)
            return null;

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
