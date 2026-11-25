
using UnityEngine;

public class TowerSectionHover : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverScaleMultiplier = 1.1f;
    [SerializeField] private float hoverSpeed = 10f;

    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Collider2D sectionCollider;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovered;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (sectionCollider == null)
            sectionCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (mainCamera == null || sectionCollider == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        isHovered = sectionCollider.OverlapPoint(mousePosition);

        targetScale = isHovered
            ? originalScale * hoverScaleMultiplier
            : originalScale;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            hoverSpeed * Time.deltaTime
        );
    }
}
