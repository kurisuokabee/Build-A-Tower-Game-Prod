using UnityEngine;

public class OrbAimController : MonoBehaviour
{
    private Camera _mainCamera;
    public Vector3 TargetWorldPosition { get; private set; }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -_mainCamera.transform.position.z;
        TargetWorldPosition = _mainCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 direction = (TargetWorldPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
}