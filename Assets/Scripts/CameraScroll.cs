
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScroll : MonoBehaviour
{
    [Header("Scroll Settings")]
    [SerializeField] private float scrollSpeed = 5f;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;

    [Header("Camera Limits")]
    [SerializeField] private float groundLimit = 0f;
    [SerializeField] private float ceilingLimit = 20f;

    private float lastMouseY;

    private void Update()
    {
        if (Mouse.current == null)
            return;

        HandleScroll();
        HandlePan();
    }

    private void HandleScroll()
    {
        float scrollInput = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scrollInput) < 0.01f)
            return;

        float movement = Mathf.Sign(scrollInput) * scrollSpeed;

        Vector3 position = transform.position;

        position.y = Mathf.Clamp(
            position.y + movement,
            groundLimit,
            ceilingLimit
        );

        transform.position = position;
    }

    
    private void HandlePan()
    {
        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            lastMouseY = Mouse.current.position.ReadValue().y;
        }

        if (!Mouse.current.middleButton.isPressed)
            return;

        float currentMouseY = Mouse.current.position.ReadValue().y;
        float mouseDeltaY = currentMouseY - lastMouseY;

        Vector3 position = transform.position;

        // Move the camera opposite to the mouse.
        position.y -= mouseDeltaY * panSpeed;

        position.y = Mathf.Clamp(
            position.y,
            groundLimit,
            ceilingLimit
        );

        transform.position = position;

        lastMouseY = currentMouseY;
    }

}
