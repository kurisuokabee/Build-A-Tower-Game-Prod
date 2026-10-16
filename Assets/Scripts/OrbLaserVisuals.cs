using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class OrbLaserVisuals : MonoBehaviour
{
    [SerializeField] private Transform muzzlePoint;
    private LineRenderer _lineRenderer;
    private OrbAimController _aimController;
    private OrbLaserCombat _laserCombat;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _aimController = GetComponent<OrbAimController>();
        _laserCombat = GetComponent<OrbLaserCombat>();
    }

    private void Update()
    {
        if (_laserCombat != null && !_laserCombat.IsFiring)
        {
            _lineRenderer.enabled = false;
            return;
        }

        _lineRenderer.enabled = true;

        Vector3 startPos = muzzlePoint != null ? muzzlePoint.position : transform.position;
        Vector3 endPos = _aimController.TargetWorldPosition;
        endPos.z = 0f;

        _lineRenderer.SetPosition(0, startPos);
        _lineRenderer.SetPosition(1, endPos);
    }
}