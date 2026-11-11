MINOR EDITS TO ALREADY EXISTING SCRIPTS

1. EnemySpawner.cs
// add these
[Header("Horde Scaling")]
private bool isHordeActive = false;

public void SetHordeActive(bool active)
{
    isHordeActive = active;
}

// update Update()
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

// new method
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
    float baseDmg = attack != null ? attack.Damage : 10f;

    float scaledHealth = baseHealth * (1f + (difficulty / 2f) * 0.1f);
    float scaledDamage = baseDmg * (1f + (difficulty / 2f) * 0.1f);

    if (health != null) health.InitializeHealth(scaledHealth);
    if (attack != null) attack.SetDamage(scaledDamage);

    TowerSection target = FindTarget(enemyObj.transform.position);
    EnemySetup setup = enemyObj.GetComponent<EnemySetup>();
    if (setup != null)
    {
        setup.Initialize(data.attackType, target, data.sprite);
    }
}


2. DebrisSpawner.cs

// add new fields and methods
private bool isSpawningEnabled = true;

public void SetSpawningEnabled(bool enabled)
{
    isSpawningEnabled = enabled;
}

public void ClearAllDebris()
{
    // Find all active debris objects tagged or tracked and return them to pool
    Debris[] allDebris = FindObjectsByType<Debris>(FindObjectsSortMode.None);
    foreach (var d in allDebris)
    {
        if (d != null && d.gameObject.activeInHierarchy)
        {
            PooledObject pooled = d.GetComponent<PooledObject>();
            if (pooled != null) pooled.ReturnToPool();
            else ObjectPoolManager.Instance?.ReturnObject(d.gameObject);
        }
    }
}

// update Update() again
private void Update()
{
    if (!isSpawningEnabled)
        return;

    spawnTimer += Time.deltaTime;

    if (spawnTimer >= spawnInterval)
    {
        spawnTimer -= spawnInterval;
        SpawnDebris();
    }
}

