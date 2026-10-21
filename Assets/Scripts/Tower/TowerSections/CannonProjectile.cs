using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float travelDuration = 1.2f;

    private float areaOfEffect = 2f;
    private Vector3 startPosition;
    private Enemy target;
    float damage;
    private float travelTimer;

    public void Initialize(Enemy target, float damage, float aoe)
    {
        startPosition = transform.position;

        this.target = target;
        this.damage = damage;
        areaOfEffect = aoe;
    }

    void Update()
    {
        CannonArcMovement();
    }

    private void CannonArcMovement()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Explode();
            return;
        }

        if (travelDuration <= 0f)
        {
            float distance = Vector3.Distance(
                startPosition,
                target.transform.position
            );

            travelDuration = Mathf.Max(distance / speed, 0.05f);
            travelTimer = 0f;
        }

        travelTimer += Time.deltaTime;

        float t = Mathf.Clamp01(travelTimer / travelDuration);
        Vector3 endPosition = target.transform.position;

        Vector3 position = Vector3.Lerp(startPosition, endPosition, t);
        position.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = position;

        Vector3 direction = endPosition - startPosition;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (t >= 1f)
            Explode();
    }

    private void Explode()
    {
        DamageAllInRange(
            transform.position,
            areaOfEffect,
            damage
        );

        Destroy(gameObject);
    }

    void DamageAllInRange(
        Vector2 center,
        float radius,
        float damage)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        float radiusSquared = radius * radius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 offset =
                (Vector2)enemy.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
                enemy.TakeDamage(damage);
        }
    }
}