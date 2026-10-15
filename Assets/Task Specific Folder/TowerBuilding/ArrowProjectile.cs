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


    public void Initialize(Enemy target) 
    {
        startPosition = transform.position;

        this.target = target;
    }

    private void Update()
    {
        ArrowMovement();
    }

    private void ArrowMovement()
    {
        timer += Time.deltaTime * speed;
        float t = Mathf.Clamp01(timer / flightDuration);

        transform.position = Vector3.Lerp(startPosition, target.transform.position, t);

        Vector3 direction = target.transform.position - transform.position;
        if(direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        if (t >= 1f)
            Destroy(gameObject);
    }
}
