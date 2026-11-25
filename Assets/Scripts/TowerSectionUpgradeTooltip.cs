
using System.Collections;
using UnityEngine;

public class TowerSectionUpgradeTooltip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TowerSectionUpgradeTree upgradeTree;
    [SerializeField] private Collider2D sectionCollider;
    [SerializeField] private Camera mainCamera;

    [Header("Tooltip Settings")]
    [SerializeField] private float hideDelay = 0.15f;

    private bool isHovered;
    private bool tooltipVisible;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (upgradeTree == null)
            upgradeTree = GetComponent<TowerSectionUpgradeTree>();

        if (sectionCollider == null)
            sectionCollider = GetComponent<Collider2D>();

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null || sectionCollider == null)
            return;

        Vector2 mousePosition =
            mainCamera.ScreenToWorldPoint(Input.mousePosition);

        bool hovering = sectionCollider.OverlapPoint(mousePosition);

        if (hovering)
        {
            if (!isHovered)
            {
                isHovered = true;

                if (hideCoroutine != null)
                {
                    StopCoroutine(hideCoroutine);
                    hideCoroutine = null;
                }

                ShowUpgradeTooltip();
            }
        }
        else if (isHovered)
        {
            isHovered = false;

            if (hideCoroutine != null)
                StopCoroutine(hideCoroutine);

            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
    }

    private void ShowUpgradeTooltip()
    {
        if (upgradeTree == null ||
            !upgradeTree.HasChosenBranch ||
            TooltipManager.Instance == null)
        {
            return;
        }

        string title = upgradeTree.GetUpgradeName(
            upgradeTree.SelectedBranch
        );

        string description = upgradeTree.GetUpgradeDescription(
            upgradeTree.SelectedBranch
        );

        if (upgradeTree.HasChosenSpecialization)
        {
            title += " - " + upgradeTree.GetUpgradeName(
                upgradeTree.SelectedSpecialization
            );

            description += "\n\n" +
                upgradeTree.GetUpgradeDescription(
                    upgradeTree.SelectedSpecialization
                );
        }

        TooltipManager.Instance.ShowUpgradeTooltip(
            title,
            description,
            this
        );

        tooltipVisible = true;
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSecondsRealtime(hideDelay);

        HideUpgradeTooltip();
        hideCoroutine = null;
    }

    private void HideUpgradeTooltip()
    {
        if (!tooltipVisible)
            return;

        if (TooltipManager.Instance != null)
            TooltipManager.Instance.HideTooltip(this);

        tooltipVisible = false;
    }

    private void OnDisable()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        isHovered = false;
        HideUpgradeTooltip();
    }
}
