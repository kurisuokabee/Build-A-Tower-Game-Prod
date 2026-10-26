
using UnityEngine;

public class SakeroCombat : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float attacksPerSecond = 2f;
    [SerializeField] private float aoeRadius = 1f;

    [Header("Target Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("AOE Visual")]
    [SerializeField] private LineRenderer aoeLineRenderer;
    [SerializeField] private int circleSegments = 48;
    [SerializeField] private float lineWidth = 0.05f;

    private float tickTimer;
    private Camera mainCamera;

    public float CurrentDamage { get; private set; }
    public float CurrentAttacksPerSecond { get; private set; }
    public float CurrentAoeRadius { get; private set; }

    private void Awake()
    {
        CurrentDamage = baseDamage;
        CurrentAttacksPerSecond = attacksPerSecond;
        CurrentAoeRadius = aoeRadius;

        mainCamera = Camera.main;

        if (aoeLineRenderer != null)
        {
            aoeLineRenderer.positionCount = circleSegments;
            aoeLineRenderer.useWorldSpace = true;
            aoeLineRenderer.loop = true;
            aoeLineRenderer.startWidth = lineWidth;
            aoeLineRenderer.endWidth = lineWidth;
            aoeLineRenderer.enabled = true;
        }
    }

    private void Update()
    {
        if (mainCamera == null || CurrentAttacksPerSecond <= 0f)
            return;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCamera.transform.position.z;

        Vector2 mouseWorldPos =
            mainCamera.ScreenToWorldPoint(mouseScreenPos);

        UpdateAoeVisual(mouseWorldPos);

        // Only activate the AOE while hovering over an enemy.
        if (!IsCursorOnEnemy(mouseWorldPos))
        {
            tickTimer = 0f;
            return;
        }

        tickTimer += Time.deltaTime;

        float interval = 1f / CurrentAttacksPerSecond;

        if (tickTimer >= interval)
        {
            tickTimer -= interval;

            DamageEnemiesInAoe(
                mouseWorldPos,
                CurrentAoeRadius,
                CurrentDamage
            );
        }
    }

    private bool IsCursorOnEnemy(Vector2 cursorPosition)
    {
        Collider2D hit = Physics2D.OverlapPoint(
            cursorPosition,
            enemyLayer
        );

        return hit != null &&
               hit.GetComponentInParent<EnemyHealth>() != null;
    }

    private void UpdateAoeVisual(Vector2 center)
    {
        if (aoeLineRenderer == null || circleSegments <= 0)
            return;

        float angleStep = 360f / circleSegments;

        for (int i = 0; i < circleSegments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;

            Vector3 point = new Vector3(
                center.x + Mathf.Cos(angle) * CurrentAoeRadius,
                center.y + Mathf.Sin(angle) * CurrentAoeRadius,
                0f
            );

            aoeLineRenderer.SetPosition(i, point);
        }
    }

    private void DamageEnemiesInAoe(
        Vector2 center,
        float radius,
        float damage)
    {
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        float radiusSquared = radius * radius;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 offset =
                (Vector2)enemy.transform.position - center;

            if (offset.sqrMagnitude <= radiusSquared)
                enemy.TakeDamage(damage);
        }
    }

    public void ApplyUpgrade(
        float damageIncrease,
        float attackSpeedIncrease,
        float radiusIncrease)
    {
        CurrentDamage += damageIncrease;
        CurrentAttacksPerSecond += attackSpeedIncrease;
        CurrentAoeRadius += radiusIncrease;
    }
}
