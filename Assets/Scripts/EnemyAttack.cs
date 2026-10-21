
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public enum AttackType
    {
        Melee,
        Ranged
    }

    [Header("Attack Settings")]
    [SerializeField] private AttackType attackType;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float damage = 10f;

    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;

    private TowerSection target;
    private TowerHealth towerHealth;
    private float attackTimer;

    public AttackType CurrentAttackType => attackType;
    
    public void Initialize(AttackType type, TowerSection targetSection, float range)
    {
        attackType = type;
        target = targetSection;
        attackTimer = 0f;

        attackRange = range;

        towerHealth = target.GetComponentInParent<TowerHealth>();
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        Vector3 difference = target.transform.position - transform.position;

        if (difference.sqrMagnitude > attackRange * attackRange)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        attackTimer = 0f;

        if (attackType == AttackType.Melee)
            MeleeAttack();
        else
            RangedAttack();
    }

    private void MeleeAttack()
    {
        if (towerHealth != null)
            towerHealth.TakeDamage(damage);
    }

    private void RangedAttack()
    {
        if (projectilePrefab == null)
            return;

        GameObject projectile = Instantiate(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
            enemyProjectile.Initialize(target, projectileSpeed, damage);
        else
            Destroy(projectile);
    }
}
