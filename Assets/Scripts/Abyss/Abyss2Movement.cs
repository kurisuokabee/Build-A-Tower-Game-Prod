
using UnityEngine;

public class Abyss2BossMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField, Min(0.1f)] private float minMovementSpeed = 1f;
    [SerializeField, Min(0.1f)] private float maxMovementSpeed = 3f;

    [SerializeField, Min(0.1f)] private float movementRange = 5f;

    private float movementSpeed;
    private float upperLimit;
    private float lowerLimit;
    private float direction;

    private void OnEnable()
    {
        InitializeMovement();
    }

    private void Update()
    {
        MoveVertically();
    }

    public void InitializeMovement()
    {
        movementSpeed = Random.Range(
            minMovementSpeed,
            Mathf.Max(minMovementSpeed, maxMovementSpeed)
        );

        float centerY = transform.position.y;

        upperLimit = centerY + movementRange;
        lowerLimit = centerY - movementRange;

        transform.position = new Vector3(
            transform.position.x,
            Random.Range(lowerLimit, upperLimit),
            transform.position.z
        );

        direction = Random.value < 0.5f ? -1f : 1f;
    }

    private void MoveVertically()
    {
        Vector3 position = transform.position;

        position.y += direction * movementSpeed * Time.deltaTime;

        if (position.y >= upperLimit)
        {
            position.y = upperLimit;
            direction = -1f;
        }
        else if (position.y <= lowerLimit)
        {
            position.y = lowerLimit;
            direction = 1f;
        }

        transform.position = position;
    }
}
