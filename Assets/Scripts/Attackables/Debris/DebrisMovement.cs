using UnityEngine;

public class DebrisMovement : MonoBehaviour
{
    private float fallSpeed = 3f;
    private PooledObject pooledObject;

    void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }
    public void Initialize(float speed)
    {
        fallSpeed = speed;
        
    }

    private void Update()
    {
        transform.position += fallSpeed * Time.deltaTime * Vector3.down;

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