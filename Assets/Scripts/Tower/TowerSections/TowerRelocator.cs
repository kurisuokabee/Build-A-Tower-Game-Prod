
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TowerSectionRelocator : MonoBehaviour
{
    [SerializeField] private TowerBuilding tower;
    [SerializeField] private TowerSectionGhost ghostPrefab;

    private TowerSection selectedSection;
    private TowerSectionGhost draggedGhost;

    private void Update()
    {
        if (TowerSectionUI.IsHoldingSection)
            return;

        if (TowerSectionUI.IgnoreRelocatorClick)
        {
            TowerSectionUI.ClearRelocatorClick();
            return;
        }

        if (TowerRelocationMode.Instance == null ||
            !TowerRelocationMode.Instance.IsRelocationMode)
        {
            return;
        }

        if (Mouse.current == null)
            return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            CancelMove();
            return;
        }

        if (!Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedSection != null)
                UpdateGhost();

            return;
        }

        if (IsPointerOverUI())
        {
            if (selectedSection != null)
                CancelMove();

            return;
        }

        if (selectedSection == null)
            TrySelectSection();
        else
            TrySwapSection();

        if (selectedSection != null)
            UpdateGhost();
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

        CancelMove();
    }

    private void HandleModeChanged(bool relocationEnabled)
    {
        if (!relocationEnabled)
            CancelMove();
    }

    private Vector3 MousePosition()
    {
        Vector3 position = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        position.z = 0f;
        return position;
    }

    private void TrySelectSection()
    {
        if (tower == null || ghostPrefab == null)
            return;

        Vector3 mousePosition = MousePosition();

        selectedSection = tower.GetSectionAtPosition(mousePosition);

        if (selectedSection == null)
            return;

        draggedGhost = Instantiate(ghostPrefab);
        draggedGhost.SetSprite(selectedSection);
        draggedGhost.SetAlpha(0.5f);
    }

    private void UpdateGhost()
    {
        if (draggedGhost == null)
            return;

        draggedGhost.transform.position = MousePosition();
    }

    private void TrySwapSection()
    {
        if (tower == null)
            return;

        Vector3 mousePosition = MousePosition();

        TowerSection targetSection =
            tower.GetSectionAtPosition(mousePosition, selectedSection);

        if (targetSection == null)
            return;

        tower.SwapTowerSections(selectedSection, targetSection);

        FinishMove();
    }

    private void CancelMove()
    {
        if (draggedGhost != null)
            Destroy(draggedGhost.gameObject);

        selectedSection = null;
        draggedGhost = null;
    }

    private void FinishMove()
    {
        CancelMove();
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }
}
