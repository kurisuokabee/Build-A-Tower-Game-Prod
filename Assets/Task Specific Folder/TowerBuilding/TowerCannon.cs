using UnityEngine;

public class TowerCannon : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 20f;

    [Header("Projectile")]
    [SerializeField] Transform firePoint;
    [SerializeField] CannonProjectile cannonPrefab;

    private EnemyFinder enemyFinder;
    private float attackTimer;

    private void Awake()
    {
        enemyFinder = GetComponent<EnemyFinder>();
    }

    private void Update()
    {
        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        Enemy target =
            enemyFinder.FindNearestEnemy(attackRange);

        if (target == null)
            return;

        SpawnStarProjectile(target);

        attackTimer = attackCooldown;
    }

    private void SpawnStarProjectile(Enemy target)
    {
        CannonProjectile projectile = Instantiate(
                    cannonPrefab,
                    firePoint.position,
                    Quaternion.identity
                );

        projectile.Initialize(
            target
        );
    }

}
