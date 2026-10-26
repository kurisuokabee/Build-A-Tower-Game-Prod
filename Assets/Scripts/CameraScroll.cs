
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class CameraScroll : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float maxZoomOut = 10f;
    [SerializeField] private float zoomSmoothness = 10f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;

    [Header("Camera Limits")]
    [SerializeField] private float groundLimit = 0f;
    [SerializeField] private float ceilingLimit = 20f;

    private Camera cam;
    private float originalZoom;
    private float targetZoom;
    private float lastMouseY;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        originalZoom = cam.orthographicSize;
        targetZoom = originalZoom;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        HandleScroll();
        HandleZoom();
        HandlePan();
    }

    private void HandleScroll()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) < 0.01f)
            return;

        targetZoom -= Mathf.Sign(scrollInput) * zoomSpeed;

        // Keep zoom within the original size and ceiling limit.
        float bottomY = transform.position.y - cam.orthographicSize;
        float maxAllowedZoom = Mathf.Max(
            originalZoom,
            ceilingLimit - bottomY
        );

        targetZoom = Mathf.Clamp(
            targetZoom,
            originalZoom,
            Mathf.Min(maxZoomOut, maxAllowedZoom)
        );
    }

    
    private void HandleZoom()
    {
        float oldZoom = cam.orthographicSize;

        float newZoom = Mathf.Lerp(
            oldZoom,
            targetZoom,
            zoomSmoothness * Time.deltaTime
        );

        if (Mathf.Approximately(oldZoom, newZoom))
            return;

        Vector3 position = transform.position;

        float oldTop = position.y + oldZoom;
        float oldBottom = position.y - oldZoom;

        // If the camera is at the ceiling and zooming out,
        // keep its top edge fixed.
        bool zoomingOut = newZoom > oldZoom;
        bool topAtCeiling = oldTop >= ceilingLimit - 0.01f;

        cam.orthographicSize = newZoom;

        if (zoomingOut && topAtCeiling)
        {
            position.y = ceilingLimit - newZoom;
        }
        else
        {
            // Otherwise, keep the bottom edge fixed.
            position.y = oldBottom + newZoom;
        }

        transform.position = position;
    }


    private void HandlePan()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame)
            lastMouseY = Mouse.current.position.ReadValue().y;

        if (!Mouse.current.middleButton.isPressed)
            return;

        float currentMouseY = Mouse.current.position.ReadValue().y;
        float mouseDeltaY = currentMouseY - lastMouseY;

        float worldDeltaY =
            mouseDeltaY * (2f * cam.orthographicSize) / Screen.height;

        Vector3 position = transform.position;
        position.y -= worldDeltaY * panSpeed;

        float halfHeight = cam.orthographicSize;

        // Clamp the camera's bottom and top edges.
        float minY = groundLimit + halfHeight;
        float maxY = ceilingLimit - halfHeight;

        position.y = maxY < minY
            ? minY
            : Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
        lastMouseY = currentMouseY;
    }
}
