
using UnityEngine;

public class Abyss3BossMovement : MonoBehaviour
{
    [Header("Movement Target")]
    [SerializeField] private TowerBuilding tower;

    [Header("Circular Movement")]
    [SerializeField, Min(0.1f)] private float radius = 5f;
    [SerializeField, Min(0.1f)] private float rotationSpeed = 30f;
    [SerializeField] private float heightOffset = 5f;

    [Header("Movement Shake")]
    [SerializeField] private float shakeAmount = 0.08f;
    [SerializeField] private float shakeSpeed = 40f;

    private Vector3 circleCenter;
    private float angle;
    private float shakeTimer;

    private void Start()
    {
        if (tower == null)
            tower = TowerBuilding.Instance;

        if (tower == null || tower.StartingBase == null)
        {
            Debug.LogWarning("Abyss 3 Boss Movement: Starting tower not found!");
            enabled = false;
            return;
        }

        Vector3 towerPosition = tower.StartingBase.transform.position;

        circleCenter = new Vector3(
            towerPosition.x,
            towerPosition.y + heightOffset,
            towerPosition.z
        );

        Vector3 offset = transform.position - circleCenter;

        angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }

    private void Update()
    {
        angle += rotationSpeed * Time.deltaTime;

        if (angle >= 360f)
            angle -= 360f;

        float radians = angle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(radians) * radius,
            Mathf.Sin(radians) * radius,
            0f
        );

        Vector3 movementPosition = circleCenter + offset;

        shakeTimer += Time.deltaTime * shakeSpeed;

        Vector3 shakeOffset = new Vector3(
            Random.Range(-shakeAmount, shakeAmount),
            Random.Range(-shakeAmount, shakeAmount),
            0f
        );

        transform.position = movementPosition + shakeOffset;
    }
}
