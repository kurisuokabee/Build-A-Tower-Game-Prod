using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Data")]
    [SerializeField] private EnemyData[] enemies;

    [Header("Spawn Areas")]
    [SerializeField] private SpawnArea[] spawnAreas;

    [Header("Tower")]
    [SerializeField] private TowerBuilding tower;

    //[Header("Spawn Settings")]
    //[SerializeField, Min(0.01f)] private float spawnInterval = 2f;

    private bool isHordeActive = false;

    private float spawnTimer;

    private void Update()
    {
        // Only spawn during Level 4 horde survival or if specifically enabled
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

        // Design requirement 4.2 formula: enemiesPerSecond = 1 + (difficulty / 2)
        float difficulty = AbyssManager.Instance != null ? AbyssManager.Instance.GetCurrentDifficulty() : 1f;
        float enemiesPerSecond = 1f + (difficulty / 2f);
        float interval = Mathf.Max(0.1f, 1f / enemiesPerSecond);

        if (spawnTimer >= interval)
        {
            spawnTimer -= interval;
            SpawnHordeEnemy(difficulty);
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

    private void SpawnHordeEnemy(float difficulty)
    {
        if (spawnAreas == null || spawnAreas.Length == 0 || enemies == null || enemies.Length == 0)
            return;

        EnemyData data = enemies[Random.Range(0, enemies.Length)];
        if (data == null || data.prefab == null) return;

        SpawnArea area = spawnAreas[Random.Range(0, spawnAreas.Length)];
        if (area == null) return;

        GameObject enemyObj = ObjectPoolManager.Instance != null 
            ? ObjectPoolManager.Instance.GetObject(data.prefab, area.GetRandomPosition(), Quaternion.identity)
            : Instantiate(data.prefab, area.GetRandomPosition(), Quaternion.identity);

        if (enemyObj == null) return;

        AttackableHealth health = enemyObj.GetComponent<AttackableHealth>();
        EnemyAttack attack = enemyObj.GetComponent<EnemyAttack>();

        float baseHealth = health != null ? health.MaxHealth : 50f;
        //float baseDmg = attack != null ? attack.Damage : 10f;

        float scaledHealth = baseHealth * (1f + (difficulty / 2f) * 0.1f);
        //float scaledDamage = baseDmg * (1f + (difficulty / 2f) * 0.1f);

        if (health != null) health.InitializeHealth(scaledHealth);
        //if (attack != null) attack.SetDamage(scaledDamage);

        TowerSection target = FindTarget(enemyObj.transform.position);
        EnemySetup setup = enemyObj.GetComponent<EnemySetup>();
        if (setup != null)
        {
            setup.Initialize(data.attackType, target, data.sprite);
        }
    }

    public void SetHordeActive(bool active)
    {
        isHordeActive = active;
    }
}
