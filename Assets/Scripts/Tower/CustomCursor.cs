using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform cursorRect;
    [SerializeField] private Canvas canvas;

    [Header("Rotation")]
    [SerializeField] private float rotationAngle = 20f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Click Scale")]
    [SerializeField] private float clickScale = 0.9f;
    [SerializeField] private float scaleSpeed = 15f;

    private Quaternion originalRotation;
    private Vector3 originalScale;

    private void Start()
    {
        Cursor.visible = false;

        originalRotation = cursorRect.localRotation;
        originalScale = cursorRect.localScale;
    }

    private void Update()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.worldCamera,
            out Vector2 localPoint))
        {
            cursorRect.localPosition = localPoint;
        }

        bool clicking = Input.GetMouseButton(0);

        Quaternion targetRotation = clicking
            ? originalRotation * Quaternion.Euler(0f, 0f, rotationAngle)
            : originalRotation;

        Vector3 targetScale = clicking
            ? originalScale * clickScale
            : originalScale;

        cursorRect.localRotation = Quaternion.Lerp(
            cursorRect.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        cursorRect.localScale = Vector3.Lerp(
            cursorRect.localScale,
            targetScale,
            scaleSpeed * Time.deltaTime
        );
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }
}