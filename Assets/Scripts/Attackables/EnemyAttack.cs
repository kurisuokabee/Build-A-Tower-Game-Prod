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
    [SerializeField] private int damage = 10;

    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;

    private TowerSection target;
    private AttackableHealth attackableHealth;
    private float attackTimer;
    private EnemyMovement enemyMovement;
    public AttackType CurrentAttackType => attackType;

    private void Awake()
    {
        attackableHealth = GetComponent<AttackableHealth>();
        enemyMovement = GetComponent<EnemyMovement>();
    }

    public void Initialize(
        AttackType type,
        TowerSection targetSection,
        float range)
    {
        attackType = type;
        target = targetSection;
        attackTimer = 0f;
        attackRange = range;
    }

    private void Update()
    {
        if (attackableHealth != null && attackableHealth.IsStunned)
            return;

        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        if (enemyMovement != null && !enemyMovement.IsInAttackRange)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        bool attacked = attackType == AttackType.Melee
            ? MeleeAttack()
            : RangedAttack();

        if (attacked)
            attackTimer = 0f;
    }

    private bool MeleeAttack()
    {
        if (AbyssManager.Instance == null)
            return false;

        AbyssManager.Instance.DamageTowerBlocks(damage);
        return true;
    }

    private bool RangedAttack()
    {
        if (projectilePrefab == null || ObjectPoolManager.Instance == null)
            return false;

        GameObject projectile = ObjectPoolManager.Instance.GetObject(
            projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        if (projectile == null)
            return false;

        EnemyProjectile enemyProjectile =
            projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile == null)
        {
            ObjectPoolManager.Instance.ReturnObject(projectile);
            return false;
        }

        enemyProjectile.Initialize(target, projectileSpeed, damage);
        return true;
    }
}
