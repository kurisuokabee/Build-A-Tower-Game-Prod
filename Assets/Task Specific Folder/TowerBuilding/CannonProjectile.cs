using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private float speed = 2f;
    [SerializeField] private float travelDuration = 1.2f;
    private Vector3 startPosition;
    private Enemy target;
    private float timer;
    private float travelTimer;

    public void Initialize(Enemy target)
    {
        startPosition = transform.position;

        this.target = target;

    }

    void Update()
    {
        CannonArcMovement();
    }

    private void CannonArcMovement()
    {
        if (travelDuration <= 0f)
        {
            startPosition = transform.position;

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

        // Move in an arc toward the target.
        Vector3 position = Vector3.Lerp(
            startPosition,
            endPosition,
            t
        );

        position.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

        transform.position = position;

        Vector3 direction = endPosition - startPosition;

        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        if (t >= 1f)
            Destroy(gameObject);
            //HitTarget();
    }
}