
using UnityEngine;

public class SpawnArea : MonoBehaviour
{
    [SerializeField] private Vector2 areaSize = new Vector2(5f, 3f);

    public Vector3 GetRandomPosition()
    {
        return GetRandomPositionInRange(
            transform.position.y - areaSize.y / 2f,
            transform.position.y + areaSize.y / 2f
        );
    }

    public Vector3 GetRandomPositionInRange(
        float minY,
        float maxY)
    {
        float x = Random.Range(
            -areaSize.x / 2f,
            areaSize.x / 2f
        );

        float y = Random.Range(minY, maxY);

        return new Vector3(
            transform.position.x + x,
            y,
            transform.position.z
        );
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
