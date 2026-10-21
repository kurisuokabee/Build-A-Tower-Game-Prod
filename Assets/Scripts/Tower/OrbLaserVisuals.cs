
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(LineRenderer))]
public class OrbLaserVisuals : MonoBehaviour
{
    [SerializeField] private Transform muzzlePoint;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
    }

    private void Update()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        Vector3 startPos = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position;

        startPos.z = 0f;
        _lineRenderer.SetPosition(0, startPos);

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;

        Vector3 mouseWorldPos =
            Camera.main.ScreenToWorldPoint(mouseScreenPos);

        mouseWorldPos.z = 0f;
        _lineRenderer.SetPosition(1, mouseWorldPos);
    }
}
