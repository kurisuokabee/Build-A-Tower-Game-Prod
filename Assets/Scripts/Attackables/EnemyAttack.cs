
using System.Collections;
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
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private int damage = 10;

    [Header("Ranged Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 6f;

    [Header("Rotation")]
    [SerializeField] private bool rotateTowardsTarget = true;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Ranged Animation")]
    [SerializeField] private float rangedSquash = 0.7f;
    [SerializeField] private float rangedStretch = 1.3f;
    [SerializeField] private float rangedAnimationDuration = 0.2f;

    [Header("Melee Animation")]
    [SerializeField] private float meleeRotationAngle = 25f;
    [SerializeField] private float meleeAnimationDuration = 0.2f;

    private TowerSection target;
    private AttackableHealth attackableHealth;
    private float attackTimer;
    private EnemyMovement enemyMovement;

    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Coroutine attackAnimation;

    public AttackType CurrentAttackType => attackType;
    public int Damage => damage;

    private void Awake()
    {
        attackableHealth = GetComponent<AttackableHealth>();
        enemyMovement = GetComponent<EnemyMovement>();

        originalScale = transform.localScale;
        originalRotation = transform.rotation;
    }

    private void OnEnable()
    {
        attackTimer = 0f;
        target = null;

        transform.localScale = originalScale;
    }

    public void Initialize(
        AttackType type,
        TowerSection targetSection)
    {
        attackType = type;
        target = targetSection;
        attackTimer = 0f;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
            return;

        if (attackableHealth != null && attackableHealth.IsStunned)
            return;

        RotateTowardsTarget();

        if (enemyMovement != null && !enemyMovement.IsInAttackRange)
            return;

        attackTimer += Time.deltaTime;

        if (attackTimer < attackInterval)
            return;

        bool attacked = attackType == AttackType.Melee
            ? MeleeAttack()
            : RangedAttack();

        if (attacked)
        {
            attackTimer = 0f;
            PlayAttackAnimation();
        }
    }

    private void RotateTowardsTarget()
    {
        if (!rotateTowardsTarget || target == null)
            return;

        Vector2 direction =
            (Vector2)(target.transform.position - transform.position);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
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

    private void PlayAttackAnimation()
    {
        if (attackAnimation != null)
            StopCoroutine(attackAnimation);

        attackAnimation = attackType == AttackType.Ranged
            ? StartCoroutine(RangedAttackAnimation())
            : StartCoroutine(MeleeAttackAnimation());
    }

    private IEnumerator RangedAttackAnimation()
    {
        Vector3 startScale = transform.localScale;

        Vector3 squashedScale = new Vector3(
            originalScale.x * rangedSquash,
            originalScale.y ,
            originalScale.z
        );

        Vector3 stretchedScale = new Vector3(
            originalScale.x * rangedStretch,
            originalScale.y ,
            originalScale.z
        );

        float halfDuration = rangedAnimationDuration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            transform.localScale = Vector3.Lerp(
                startScale,
                squashedScale,
                t
            );

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            transform.localScale = Vector3.Lerp(
                squashedScale,
                stretchedScale,
                t
            );

            yield return null;
        }

        transform.localScale = originalScale;
        attackAnimation = null;
    }

    private IEnumerator MeleeAttackAnimation()
    {
        Quaternion startingRotation = transform.rotation;

        Quaternion backwardRotation =
            startingRotation * Quaternion.Euler(
                0f, 0f, -meleeRotationAngle
            );

        Quaternion forwardRotation =
            startingRotation * Quaternion.Euler(
                0f, 0f, meleeRotationAngle
            );

        float halfDuration = meleeAnimationDuration * 0.5f;
        float timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            transform.rotation = Quaternion.Slerp(
                startingRotation,
                backwardRotation,
                t
            );

            yield return null;
        }

        timer = 0f;

        while (timer < halfDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / halfDuration);

            transform.rotation = Quaternion.Slerp(
                backwardRotation,
                forwardRotation,
                t
            );

            yield return null;
        }

        transform.rotation = startingRotation;
        attackAnimation = null;
    }

    private void OnDisable()
    {
        if (attackAnimation != null)
        {
            StopCoroutine(attackAnimation);
            attackAnimation = null;
        }

        transform.localScale = originalScale;
    }
}
