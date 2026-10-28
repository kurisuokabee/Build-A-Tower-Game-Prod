using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;

   
    private TowerSection targetSection;

    public bool IsInAttackRange { get; private set; }

    private void Update()
    {   
        if (targetSection == null)
            return;

        MoveToTarget();
    }

    
    private void MoveToTarget()
    {
        if (targetSection == null)
            return;

        Vector3 targetPosition = targetSection.transform.position;

        float distanceSquared =
            (targetPosition - transform.position).sqrMagnitude;

        IsInAttackRange = distanceSquared <= attackRange * attackRange;

        if (IsInAttackRange)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );
    }


    public void Initialize(TowerSection target, float range)
    {
        targetSection = target;
        attackRange = range;
    }

       
}
