
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData[] enemies;

    [Header("Spawn Areas")]
    [SerializeField] private SpawnArea[] spawnAreas;

    [Header("Tower")]
    [SerializeField] private TowerBuilding tower;

    private bool isHordeActive;
    private float spawnTimer;

    private void Update()
    {
        if (!isHordeActive)
        {
            spawnTimer = 0f;
            return;
        }

        if (tower == null || tower.Sections.Count == 0)
        {
            spawnTimer = 0f;
            return;
        }

        spawnTimer += Time.deltaTime;

        float difficulty = AbyssManager.Instance != null
            ? AbyssManager.Instance.GetCurrentDifficulty()
            : 1f;

        float interval = Mathf.Max(2f, 15f - (difficulty * 0.1f));

        if (spawnTimer >= interval)
        {
            spawnTimer -= interval;
            SpawnHordeEnemy(difficulty);
        }
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

    private void SpawnHordeEnemy(float difficulty)
    {
        if (spawnAreas == null || spawnAreas.Length == 0 ||
            enemies == null || enemies.Length == 0)
            return;

        EnemyData data = enemies[Random.Range(0, enemies.Length)];

        if (data == null || data.prefab == null)
            return;

        SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Length)];

        if (area == null)
            return;

        Vector3 spawnPosition = area.GetRandomPosition();

        GameObject enemyObj = ObjectPoolManager.Instance != null
            ? ObjectPoolManager.Instance.GetObject(
                data.prefab,
                spawnPosition,
                Quaternion.identity)
            : Instantiate(data.prefab, spawnPosition, Quaternion.identity);

        if (enemyObj == null)
            return;

        AttackableHealth health = enemyObj.GetComponent<AttackableHealth>();
        
        if (health != null)
        {
            float scaledHealth = health.MaxHealth *
                (1f + (difficulty / 2f) * 0.1f);

            health.InitializeHealth(scaledHealth);
        }
 
        TowerSection target = FindTarget(spawnPosition);

        EnemySetup setup = enemyObj.GetComponent<EnemySetup>();

        if (setup != null)
        {
            setup.Initialize(
                data,
                target
            );
        }
    }

    public void SetHordeActive(bool active)
    {
        isHordeActive = active;
        spawnTimer = 0f;
    }
}
