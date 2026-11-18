using UnityEngine;

public class DebrisMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallSpeed = 3f;

    [Header("Rotation")]
    [SerializeField] private Vector2 rotationSpeedRange = new Vector2(15f, 40f);

    [Header("Scale")]
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);

    private PooledObject pooledObject;
    private AttackableHealth attackableHealth;

    private float rotationSpeed;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        attackableHealth = GetComponent<AttackableHealth>();
    }

    public void Initialize(float speed)
    {
        fallSpeed = speed;

        rotationSpeed = Random.Range(
            rotationSpeedRange.x,
            rotationSpeedRange.y
        );

        if (Random.value > 0.5f)
            rotationSpeed *= -1f;

        float randomScale = Random.Range(
            scaleRange.x,
            scaleRange.y
        );

        transform.localScale = Vector3.one * randomScale;
    }

    private void Update()
    {
        if (attackableHealth != null && attackableHealth.IsStunned)
            return;

        transform.position +=
            fallSpeed * Time.deltaTime * Vector3.down;

        transform.Rotate(
            Vector3.forward,
            rotationSpeed * Time.deltaTime
        );

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