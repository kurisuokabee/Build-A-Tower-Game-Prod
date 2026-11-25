
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;

    private TowerSection targetSection;
    private AttackableHealth attackableHealth;
    private AttackableSlow attackableSlow;

    public bool IsInAttackRange { get; private set; }

    private void Awake()
    {
        attackableHealth = GetComponent<AttackableHealth>();
        attackableSlow = GetComponent<AttackableSlow>();
    }

    private void OnEnable()
    {
        targetSection = null;
        IsInAttackRange = false;
    }

    private void OnDisable()
    {
        targetSection = null;
        IsInAttackRange = false;
    }

    private void Update()
    {
        if (attackableHealth != null && attackableHealth.IsStunned)
            return;

        if (targetSection == null || !targetSection.gameObject.activeInHierarchy)
        {
            IsInAttackRange = false;
            return;
        }

        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Vector3 targetPosition = targetSection.transform.position;

        float distanceSquared =
            (targetPosition - transform.position).sqrMagnitude;

        IsInAttackRange = distanceSquared <= attackRange * attackRange;

        if (IsInAttackRange)
            return;

        float speedMultiplier = attackableSlow != null
            ? attackableSlow.CurrentSpeedMultiplier
            : 1f;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * speedMultiplier * Time.deltaTime
        );
    }

    public void Initialize(TowerSection target, float range)
    {
        targetSection = target;
        attackRange = range;
        IsInAttackRange = false;
    }
}
