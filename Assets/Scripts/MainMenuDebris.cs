
using UnityEngine;

public class MainMenuDebris : MonoBehaviour
{
    private float moveSpeed;
    private float rotationSpeed;
    private float destroyY;

    public void Initialize(float speed, float rotation, float despawnHeight)
    {
        moveSpeed = speed;
        rotationSpeed = rotation;
        destroyY = despawnHeight;
    }

    private void Update()
    {
        transform.position += Vector3.up * (moveSpeed * Time.deltaTime);

        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        if (transform.position.y >= destroyY)
            Destroy(gameObject);
    }
}
