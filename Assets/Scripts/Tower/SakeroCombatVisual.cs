
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class SakeroCombatVisual : MonoBehaviour
{
    [Header("Laser Targeting")]
    [SerializeField] private float enemyHoverRadius = 0.5f;
    //[SerializeField] private float debrisHoverRadius = 0.5f;

    [Header("Laser Origin")]
    [SerializeField] private Transform muzzlePoint;

    private LineRenderer _lineRenderer;
    private Camera mainCamera;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
        _lineRenderer.enabled = false;

        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            _lineRenderer.enabled = false;
            return;
        }

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -mainCamera.transform.position.z;

        Vector2 mouseWorldPos =
            mainCamera.ScreenToWorldPoint(mouseScreenPos);

        if (!IsHoveringOverTarget(mouseWorldPos))
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        Vector3 startPos = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position;

        startPos.z = 0f;

        Vector3 endPos = mouseWorldPos;
        endPos.z = 0f;

        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);
    }

    private bool IsHoveringOverTarget(Vector2 mouseWorldPos)
    {
        // Check enemies.
        EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(
            FindObjectsSortMode.None
        );

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            Vector2 offset =
                (Vector2)enemy.transform.position - mouseWorldPos;

            if (offset.sqrMagnitude <=
                enemyHoverRadius * enemyHoverRadius)
            {
                return true;
            }
        }

        // Check debris.
        // Debris[] debrisObjects = FindObjectsByType<Debris>(
        //     FindObjectsSortMode.None
        // );

        // foreach (Debris debris in debrisObjects)
        // {
        //     if (debris == null || !debris.gameObject.activeInHierarchy)
        //         continue;

        //     Vector2 offset =
        //         (Vector2)debris.transform.position - mouseWorldPos;

        //     if (offset.sqrMagnitude <=
        //         debrisHoverRadius * debrisHoverRadius)
        //     {
        //         return true;
        //     }
        // }

        return false;
    }
}
