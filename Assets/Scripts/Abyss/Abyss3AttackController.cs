
using UnityEngine;

public class Abyss3AttackController : MonoBehaviour
{
    [Header("Minion Spawning")]
    [SerializeField] private EnemyData[] minionEnemies;
    [SerializeField, Min(0.1f)] private float minSpawnInterval = 2f;
    [SerializeField, Min(0.1f)] private float maxSpawnInterval = 4f;

    [Header("Tower")]
    [SerializeField] private TowerBuilding tower;

    private Abyss3Boss boss;
    private float spawnTimer;
    private float currentSpawnInterval;

    private void Awake()
    {
        boss = GetComponent<Abyss3Boss>();

        if (tower == null)
            tower = TowerBuilding.Instance;
    }

    private void Start()
    {
        ResetSpawnTimer();
    }

    private void Update()
    {
        if (boss == null || boss.IsDefeated)
            return;

        if (minionEnemies == null || minionEnemies.Length == 0)
            return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnInterval)
        {
            spawnTimer = 0f;

            SpawnMinion();
            ResetSpawnTimer();
        }
    }

    private void SpawnMinion()
    {
        EnemyData data = minionEnemies[Random.Range(0, minionEnemies.Length)];

        if (data == null || data.prefab == null)
            return;

        GameObject enemyObj = ObjectPoolManager.Instance != null
            ? ObjectPoolManager.Instance.GetObject(
                data.prefab,
                transform.position,
                Quaternion.identity)
            : Instantiate(data.prefab, transform.position, Quaternion.identity);

        if (enemyObj == null)
            return;

        Abyss3Minion minion = enemyObj.GetComponent<Abyss3Minion>();

        if (minion == null)
            minion = enemyObj.AddComponent<Abyss3Minion>();

        minion.Initialize(boss);

        TowerSection target = FindTarget(enemyObj.transform.position);

        EnemySetup setup = enemyObj.GetComponent<EnemySetup>();

        if (setup != null)
            setup.Initialize(data.attackType, target, data.sprite);
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

    private void ResetSpawnTimer()
    {
        float min = Mathf.Min(minSpawnInterval, maxSpawnInterval);
        float max = Mathf.Max(minSpawnInterval, maxSpawnInterval);

        currentSpawnInterval = Random.Range(min, max);
    }
}
