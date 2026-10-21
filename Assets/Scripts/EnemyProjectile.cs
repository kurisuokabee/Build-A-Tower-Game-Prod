
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private TowerSection target;
    private float moveSpeed;
    private float damage;

    public void Initialize(
        TowerSection targetSection,
        float speed,
        float damageAmount)
    {
        target = targetSection;
        moveSpeed = speed;
        damage = damageAmount;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if ((targetPosition - transform.position).sqrMagnitude <= 0.01f)
        {
            TowerHealth health = target.GetComponentInParent<TowerHealth>();

            if (health != null)
                health.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
