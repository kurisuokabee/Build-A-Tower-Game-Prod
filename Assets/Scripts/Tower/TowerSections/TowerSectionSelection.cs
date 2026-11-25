
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TowerSectionSelection : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    private TowerSection selectedSection;
    private SakeroWand selectedWand;

    private TowerSectionUpgradeTreeUI upgradeUI;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        upgradeUI = FindAnyObjectByType<TowerSectionUpgradeTreeUI>();

        if (upgradeUI == null)
            Debug.LogError("TowerSectionUpgradeTreeUI not found!");
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
            return;

        SelectObject();
    }

    private void SelectObject()
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

        // Check for Sakero Wand first.
        SakeroWand wand =
            hit.collider.GetComponentInParent<SakeroWand>();

        if (wand != null)
        {
            SelectWand(wand);
            return;
        }

        // Otherwise, check for a tower section.
        TowerSection section =
            hit.collider.GetComponentInParent<TowerSection>();

        if (section != null)
        {
            SelectTowerSection(section);
            return;
        }

        ClearSelection();
    }

    private void SelectWand(SakeroWand wand)
    {
        if (selectedWand == wand)
        {
            ClearSelection();
            return;
        }

        selectedSection = null;
        selectedWand = wand;

        if (upgradeUI != null)
            upgradeUI.ShowUpgrades(wand);

        Debug.Log($"Selected Sakero Wand: {wand.name}");
    }

    private void SelectTowerSection(TowerSection section)
    {
        if (selectedSection == section)
        {
            ClearSelection();
            return;
        }

        selectedWand = null;
        selectedSection = section;

        if (upgradeUI != null)
            upgradeUI.ShowUpgrades(section);

        Debug.Log($"Selected tower section: {section.name}");
    }

    private void ClearSelection()
    {
        selectedSection = null;
        selectedWand = null;

        if (upgradeUI != null)
            upgradeUI.HideUpgrades();
    }
}
