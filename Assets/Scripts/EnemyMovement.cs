using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum MovementType
    {
        Land,
        Flying
    }

    [Header("Movement")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 1f;

    [Header("Gravity")]
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float groundY = 0f;

    private float verticalVelocity;

    private TowerSection targetSection;

    public bool IsInAttackRange { get; private set; }

    private void Update()
    {   
        ApplyGravity();

        if (targetSection == null)
            return;

        MoveToTarget();
    }

    
    private void MoveToTarget()
    {
        if (targetSection == null)
            return;

        Vector3 targetPosition = targetSection.transform.position;

        if (movementType == MovementType.Land)
            targetPosition.y = transform.position.y;

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


    private void ApplyGravity()
    {
        if (movementType != MovementType.Land)
            return;

        verticalVelocity -= gravity * Time.deltaTime;

        Vector3 position = transform.position;
        position.y += verticalVelocity * Time.deltaTime;

        if (position.y <= groundY)
        {
            position.y = groundY;
            verticalVelocity = 0f;
        }

        transform.position = position;
    }


    public void Initialize(MovementType type, TowerSection target, float range)
    {
        targetSection = target;
        movementType = type;
        attackRange = range;
    }

       
}
