using System.Collections.Generic;
using UnityEngine;

public class Abyss1BossMovement : MonoBehaviour
{
    private enum MovementStage
    {
        BackAndForth,
        Triangle,
        Square,
        Circle
    }

    [Header("References")]
    [SerializeField] private Abyss1Boss bossController;

    [Header("Movement Settings")]
    [SerializeField, Min(0.1f)] private float movementSpeed = 3f;

    [Header("Back-and-Forth Settings")]
    [SerializeField, Min(0.1f)] private float backAndForthDistance = 6f;
    [SerializeField] private float backAndForthHeight = 4f;

    [Header("Orbit Settings")]
    [SerializeField, Min(0.1f)] private float triangleRadius = 7f;
    [SerializeField, Min(0.1f)] private float squareRadius = 8f;
    [SerializeField, Min(0.1f)] private float circleRadius = 9f;
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField, Range(8, 100)] private int circleSegments = 36;

    private Transform center;
    private MovementStage currentStage;

    private readonly List<Vector3> waypoints = new();

    private int waypointIndex;

    private void Awake()
    {
        if (bossController == null)
            bossController = GetComponent<Abyss1Boss>();
    }

    private void OnEnable()
    {
        if (TowerBuilding.Instance != null &&
            TowerBuilding.Instance.StartingBase != null)
        {
            center = TowerBuilding.Instance.StartingBase.transform;
            StartMovement();
        }
    }

    private void Update()
    {
        if (center == null || waypoints.Count == 0)
            return;

        if (bossController != null && !bossController.CanMove)
            return;
            
        MoveToNextPoint();
    }

    private void MoveToNextPoint()
    {
        Vector3 target = waypoints[waypointIndex];

        RotateTowardsTarget(target);

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            movementSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) > 0.05f)
            return;

        waypointIndex++;

        if (waypointIndex >= waypoints.Count)
            AdvanceStage();
    }

    private void StartMovement()
    {
        currentStage = MovementStage.BackAndForth;
        BuildBackAndForthPath();
    }
    private void RotateTowardsTarget(Vector3 target)
    {
        Vector2 direction = target - transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
    private void AdvanceStage()
    {
        switch (currentStage)
        {
            case MovementStage.BackAndForth:
                currentStage = MovementStage.Triangle;
                BuildPolygonPath(3, triangleRadius);
                break;

            case MovementStage.Triangle:
                currentStage = MovementStage.Square;
                BuildPolygonPath(4, squareRadius);
                break;

            case MovementStage.Square:
                currentStage = MovementStage.Circle;
                BuildCirclePath();
                break;

            case MovementStage.Circle:
                currentStage = MovementStage.BackAndForth;
                BuildBackAndForthPath();
                break;
        }
    }

    private void BuildBackAndForthPath()
    {
        waypoints.Clear();

        Vector2 pointA = new Vector2(
            -backAndForthDistance,
            backAndForthHeight
        );

        Vector2 pointB = new Vector2(
            backAndForthDistance,
            backAndForthHeight
        );

        waypoints.Add(GetWorldPosition(pointA));
        waypoints.Add(GetWorldPosition(pointB));
        waypoints.Add(GetWorldPosition(pointA));

        waypointIndex = 0;
    }

    private void BuildPolygonPath(int sides, float radius)
    {
        waypoints.Clear();

        float angleStep = 360f / sides;
        float startAngle = -90f;

        for (int i = 0; i <= sides; i++)
        {
            float angle = (startAngle + angleStep * i) * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * radius;

            waypoints.Add(GetWorldPosition(offset));
        }

        waypointIndex = 0;
    }

    private void BuildCirclePath()
    {
        waypoints.Clear();

        float startAngle = -90f;

        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = (
                startAngle + (360f / circleSegments) * i
            ) * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(
                Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * circleRadius;

            waypoints.Add(GetWorldPosition(offset));
        }

        waypointIndex = 0;
    }

    private Vector3 GetWorldPosition(Vector2 offset)
    {
        Vector3 position = center.position + new Vector3(
            offset.x,
            offset.y,
            0f
        );

        position.z = transform.position.z;

        return position;
    }
}
