
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerSectionSelection : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private TowerSection selectedSection;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (TowerRelocationMode.Instance != null)
            TowerRelocationMode.Instance.OnModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        if (TowerRelocationMode.Instance != null)
            TowerRelocationMode.Instance.OnModeChanged -= HandleModeChanged;
    }

    private void HandleModeChanged(bool relocationEnabled)
    {
        if (relocationEnabled)
            ClearSelection();
    }

    private void Update()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        if (TowerRelocationMode.Instance != null &&
            TowerRelocationMode.Instance.IsRelocationMode)
        {
            return;
        }

        SelectTowerSection();
    }

    private void SelectTowerSection()
    {
        if (mainCamera == null)
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        RaycastHit2D hit =
            Physics2D.Raycast(worldPosition, Vector2.zero);

        if (hit.collider == null)
        {
            ClearSelection();
            return;
        }

        TowerSection section =
            hit.collider.GetComponentInParent<TowerSection>();

        if (section == null)
        {
            ClearSelection();
            return;
        }

        selectedSection = section;

        TowerSectionUpgradeTreeUI upgradeUI =
            selectedSection.GetComponentInChildren<TowerSectionUpgradeTreeUI>(
                true
            );

        if (upgradeUI != null)
            upgradeUI.ShowUpgrades(selectedSection);

        Debug.Log($"Selected tower section: {selectedSection.name}");
    }

    private void ClearSelection()
    {
        if (selectedSection != null)
        {
            TowerSectionUpgradeTreeUI upgradeUI =
                selectedSection.GetComponentInChildren<TowerSectionUpgradeTreeUI>(
                    true
                );

            if (upgradeUI != null)
                upgradeUI.ShowUpgrades(null);
        }

        selectedSection = null;
    }
}
