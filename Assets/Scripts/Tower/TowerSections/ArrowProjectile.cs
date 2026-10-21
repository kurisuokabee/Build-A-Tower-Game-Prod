using Unity.Mathematics;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float flightDuration = 1.2f;
    [SerializeField] private float speed = 2f;
    private Vector3 startPosition;
    private Enemy target;
    private float timer;

    EnemyHealth enemyHealth;
    float damage;

    public void Initialize(Enemy target, float damage) 
    {
        startPosition = transform.position;

        this.target = target;
        enemyHealth = target.GetComponent<EnemyHealth>();
        this.damage = damage;
    }

    private void Update()
    {
        ArrowMovement();
    }

    private void ArrowMovement()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            if (!FindNewTarget())
            {
                Destroy(gameObject);
                return;
            }
        }

        timer += Time.deltaTime * speed;
        float t = Mathf.Clamp01(timer / flightDuration);

        transform.position = Vector3.Lerp(
            startPosition,
            target.transform.position,
            t
        );

        Vector3 direction = target.transform.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x)
                * Mathf.Rad2Deg - 90f;

            transform.rotation = Quaternion.AngleAxis(
                angle,
                Vector3.forward
            );
        }

        if (t >= 1f)
        {
            if (enemyHealth != null)
                enemyHealth.TakeDamage(damage);

            Destroy(gameObject);
        }
    }

    private bool FindNewTarget()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        Enemy nearestEnemy = null;
        float nearestDistance = float.MaxValue;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance = (
                enemy.transform.position - transform.position
            ).sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy == null)
            return false;

        target = nearestEnemy;
        enemyHealth = target.GetComponent<EnemyHealth>();

        // Start a fresh flight toward the new target.
        startPosition = transform.position;
        timer = 0f;

        return enemyHealth != null;
    }
}
