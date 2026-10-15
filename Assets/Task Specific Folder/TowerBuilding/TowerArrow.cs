using UnityEngine;

public class TowerArrow : MonoBehaviour
{   
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 20f;

    [Header("Projectile")]
    [SerializeField] Transform firePoint;
    [SerializeField] ArrowProjectile arrowPrefab;

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


        SpawnArrowProjectile(target);

        attackTimer = attackCooldown;

    }

    void SpawnArrowProjectile(Enemy target)
    {
        for (int i = 0; i < projectileCount; i++)
        {
            float angle = 0f;

            if (projectileCount > 1)
            {
                angle = -spreadAngle / 2f
                    + spreadAngle * i / (projectileCount - 1);
            }


            ArrowProjectile projectile = Instantiate(
                arrowPrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(
                target
            );
        }
    }

}
