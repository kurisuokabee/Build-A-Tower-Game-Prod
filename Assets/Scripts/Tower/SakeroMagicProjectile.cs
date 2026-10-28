
using UnityEngine;

public class SakeroMagicProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float hitDistance = 0.15f;

    private AttackableHealth target;
    private float damage;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(AttackableHealth newTarget, float newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    private void Update()
    {
        if (target == null ||
            !target.gameObject.activeInHierarchy ||
            !target.IsAlive)
        {
            ReturnToPool();
            return;
        }

        Vector3 targetPosition = target.transform.position;
        targetPosition.z = transform.position.z;

        Vector3 direction = targetPosition - transform.position;

        float hitDistanceSquared = hitDistance * hitDistance;

        if (direction.sqrMagnitude <= hitDistanceSquared)
        {
            target.TakeDamage(damage);
            ReturnToPool();
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    private void ReturnToPool()
    {
        target = null;

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
