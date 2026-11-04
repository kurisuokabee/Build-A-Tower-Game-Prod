using UnityEngine;

public class DebrisMovement : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 3f;

    private PooledObject pooledObject;
    private AttackableHealth attackableHealth;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        attackableHealth = GetComponent<AttackableHealth>();
    }

    public void Initialize(float speed)
    {
        fallSpeed = speed;
    }

    private void Update()
    {
        if (attackableHealth != null && attackableHealth.IsStunned)
            return;

        transform.position +=
            fallSpeed * Time.deltaTime * Vector3.down;

        if (transform.position.y < -9f)
        {
            if (pooledObject != null)
                pooledObject.ReturnToPool();
            else if (ObjectPoolManager.Instance != null)
                ObjectPoolManager.Instance.ReturnObject(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}