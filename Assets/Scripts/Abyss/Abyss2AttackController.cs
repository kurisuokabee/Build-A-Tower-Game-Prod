
using UnityEngine;

public class Abyss2AttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField, Min(0.1f)] private float minAttackInterval = 2f;
    [SerializeField, Min(0.1f)] private float maxAttackInterval = 5f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 5f;
    [SerializeField, Min(0f)] private float damage = 10f;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;

    private Abyss2Clone bossClone;
    private TowerSection startingTower;
    private float attackTimer;

    private void Awake()
    {
        bossClone = GetComponent<Abyss2Clone>();
    }

    private void Start()
    {
        TowerBuilding tower = TowerBuilding.Instance;

        if (tower != null)
            startingTower = tower.StartingBase;
        
        ResetAttackTimer();
    }

    private void Update()
    {
        if (startingTower == null || bossClone == null)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            FireProjectile();
            ResetAttackTimer();
        }
    }

    private void ResetAttackTimer()
    {
        attackTimer = Random.Range(
            minAttackInterval,
            Mathf.Max(minAttackInterval, maxAttackInterval)
        );
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null)
            return;

        GameObject projectile = ObjectPoolManager.Instance.GetObject(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        if (projectile == null)
            return;

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (projectile == null)
            return;

        enemyProjectile.transform.position = projectileSpawnPoint.position;

        enemyProjectile.Initialize(
            startingTower,
            projectileSpeed,
            damage,
            bossClone.IsMainBoss
        );
    }
}
