
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private TowerSection target;
    private float moveSpeed;
    private int damage;
    private bool canDamageTower = true;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Initialize(
        TowerSection targetSection,
        float speed,
        float damageAmount,
        bool damageTower = true)
    {
        target = targetSection;
        moveSpeed = speed;
        damage = Mathf.RoundToInt(damageAmount);
        canDamageTower = damageTower;
    }

    private void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            ReturnToPool();
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
            if (canDamageTower && AbyssManager.Instance != null)
                AbyssManager.Instance.DamageTowerBlocks(damage);

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        target = null;

        if (pooledObject != null)
            pooledObject.ReturnToPool();
        else
            ObjectPoolManager.Instance?.ReturnObject(gameObject);
    }
}
