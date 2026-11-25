
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSectionUpgradeTreeUI : MonoBehaviour
{
    [Header("Upgrade Buttons")]
    [SerializeField] private Button firstButton;
    [SerializeField] private Button secondButton;

    [Header("Button Labels")]
    [SerializeField] private TMP_Text firstButtonText;
    [SerializeField] private TMP_Text secondButtonText;

    [Header("Panel")]
    [SerializeField] private GameObject upgradePanel;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Vector3 canvasOffset = Vector3.zero;

    private TowerSection currentSection;
    private TowerSectionUpgradeTree currentTree;

    private SakeroWand currentWand;
    private SakeroWandUpgradeTree currentWandTree;

    private Transform currentTarget;

    private RectTransform canvasRect;

    private bool hoveringFirstButton;
    private bool hoveringSecondButton;

    private void Awake()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("Canvas not found!");
            return;
        }

        canvasRect = canvas.GetComponent<RectTransform>();

        if (upgradePanel == null)
        {
            Debug.LogError("Upgrade Panel reference is missing!");
            return;
        }

        if (firstButton == null || secondButton == null)
        {
            Debug.LogError("Upgrade buttons are missing!");
            return;
        }

        upgradePanel.SetActive(false);

        firstButton.onClick.AddListener(OnFirstButtonClicked);
        secondButton.onClick.AddListener(OnSecondButtonClicked);
    }

    private void OnDestroy()
    {
        if (firstButton != null)
            firstButton.onClick.RemoveListener(OnFirstButtonClicked);

        if (secondButton != null)
            secondButton.onClick.RemoveListener(OnSecondButtonClicked);
    }

    private void Update()
    {
        if (!upgradePanel.activeInHierarchy)
            return;

        bool hasActiveTree =
            currentTree != null || currentWandTree != null;

        if (!hasActiveTree)
            return;

        bool firstHovered = IsMouseOver(firstButton);
        bool secondHovered = IsMouseOver(secondButton);

        if (firstHovered && !hoveringFirstButton)
            ShowUpgradeTooltip(0);

        if (secondHovered && !hoveringSecondButton)
            ShowUpgradeTooltip(1);

        if (!firstHovered && !secondHovered &&
            (hoveringFirstButton || hoveringSecondButton))
        {
            HideUpgradeTooltip();
        }

        hoveringFirstButton = firstHovered;
        hoveringSecondButton = secondHovered;
    }

    private bool IsMouseOver(Button button)
    {
        if (button == null || !button.gameObject.activeInHierarchy)
            return false;

        RectTransform rect = button.GetComponent<RectTransform>();

        if (rect == null)
            return false;

        Camera eventCamera = canvas.worldCamera != null
            ? canvas.worldCamera
            : Camera.main;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rect,
            Input.mousePosition,
            eventCamera
        );
    }

    // TOWER SECTION UPGRADES

    public void ShowUpgrades(TowerSection section)
    {
        ClearCurrentSelection();

        currentSection = section;
        currentTree = section != null
            ? section.GetComponent<TowerSectionUpgradeTree>()
            : null;

        currentTarget = section != null
            ? section.transform
            : null;

        ShowPanel(currentTree != null);
    }

    // SAKERO WAND UPGRADES

    public void ShowUpgrades(SakeroWand wand)
    {
        ClearCurrentSelection();

        currentWand = wand;
        currentWandTree = wand != null
            ? wand.GetComponent<SakeroWandUpgradeTree>()
            : null;

        currentTarget = wand != null
            ? wand.transform
            : null;

        ShowPanel(currentWandTree != null);
    }

    private void ShowPanel(bool hasTree)
    {
        hoveringFirstButton = false;
        hoveringSecondButton = false;

        HideUpgradeTooltip();

        if (upgradePanel != null)
            upgradePanel.SetActive(hasTree);

        if (hasTree)
        {
            RefreshButtons();
            UpdateCanvasPosition();
        }
    }

    private void ClearCurrentSelection()
    {
        currentSection = null;
        currentTree = null;

        currentWand = null;
        currentWandTree = null;

        currentTarget = null;
    }

    public void HideUpgrades()
    {
        ClearCurrentSelection();

        hoveringFirstButton = false;
        hoveringSecondButton = false;

        HideUpgradeTooltip();

        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    private void LateUpdate()
    {
        if (currentTarget == null ||
            currentTree == null && currentWandTree == null)
            return;

        UpdateCanvasPosition();
    }

    private void UpdateCanvasPosition()
    {
        if (canvasRect == null || currentTarget == null)
            return;

        canvasRect.position =
            currentTarget.position + canvasOffset;
    }

    // TOOLTIP

    private void ShowUpgradeTooltip(int buttonIndex)
    {
        if (TooltipManager.Instance == null)
            return;

        if (currentTree != null)
        {
            List<TowerSectionUpgradeTree.UpgradeChoice> choices =
                currentTree.GetAvailableChoices();

            if (buttonIndex >= choices.Count)
                return;

            var choice = choices[buttonIndex];

            
            TooltipManager.Instance.ShowTooltip(
                $"{currentTree.GetUpgradeName(choice)} - {currentTree.GetUpgradeCost(choice)} Gems",
                currentTree.GetUpgradeDescription(choice),this
            );
        }
        else if (currentWandTree != null)
        {
            var choice = buttonIndex == 0
                ? SakeroWandUpgradeTree.UpgradeChoice.Left
                : SakeroWandUpgradeTree.UpgradeChoice.Right;

            SakeroWandUpgradeTree.UpgradeInfo info =
                currentWandTree.GetUpgradeInfo(choice);

            if (info == null)
                return;

            
            TooltipManager.Instance.ShowTooltip(
                $"{info.upgradeName} - {info.upgradePointCost} Gems", info.description,this);
        }
    }

    private void HideUpgradeTooltip()
    {
        if (TooltipManager.Instance != null)
        {
            
            TooltipManager.Instance.HideTooltip(this);
        }
    }

    // BUTTON EVENTS

    private void OnFirstButtonClicked()
    {
        ChooseUpgrade(0);
    }

    private void OnSecondButtonClicked()
    {
        ChooseUpgrade(1);
    }

    private void ChooseUpgrade(int buttonIndex)
    {
        if (currentTree != null)
        {
            List<TowerSectionUpgradeTree.UpgradeChoice> choices =
                currentTree.GetAvailableChoices();

            if (buttonIndex >= choices.Count)
                return;

            if (currentTree.ChooseUpgrade(choices[buttonIndex]))
                RefreshButtons();
        }
        else if (currentWandTree != null)
        {
            var choice = buttonIndex == 0
                ? SakeroWandUpgradeTree.UpgradeChoice.Left
                : SakeroWandUpgradeTree.UpgradeChoice.Right;

            if (currentWandTree.ChooseUpgrade(choice))
                RefreshButtons();
        }
    }

    // BUTTON REFRESH

    private void RefreshButtons()
    {
        if (currentTree != null)
        {
            List<TowerSectionUpgradeTree.UpgradeChoice> choices =
                currentTree.GetAvailableChoices();

            SetTowerButton(firstButton, firstButtonText, choices, 0);
            SetTowerButton(secondButton, secondButtonText, choices, 1);
        }
        else if (currentWandTree != null)
        {
            bool canShowUpgrades =
                currentWandTree.SelectedUpgrade ==
                SakeroWandUpgradeTree.UpgradeChoice.None;

            SetWandButton(
                firstButton,
                firstButtonText,
                SakeroWandUpgradeTree.UpgradeChoice.Left,
                canShowUpgrades
            );

            SetWandButton(
                secondButton,
                secondButtonText,
                SakeroWandUpgradeTree.UpgradeChoice.Right,
                canShowUpgrades
            );
        }

        hoveringFirstButton = false;
        hoveringSecondButton = false;

        HideUpgradeTooltip();
    }

    private void SetTowerButton(
        Button button,
        TMP_Text label,
        List<TowerSectionUpgradeTree.UpgradeChoice> choices,
        int index)
    {
        if (button == null)
            return;

        bool available = index < choices.Count;

        button.gameObject.SetActive(available);

        if (available && label != null)
            label.text = currentTree.GetUpgradeName(choices[index]);
    }

    private void SetWandButton(
        Button button,
        TMP_Text label,
        SakeroWandUpgradeTree.UpgradeChoice choice,
        bool available)
    {
        if (button == null)
            return;

        SakeroWandUpgradeTree.UpgradeInfo info =
            currentWandTree.GetUpgradeInfo(choice);

        bool canShow = available && info != null;

        button.gameObject.SetActive(canShow);

        if (canShow && label != null)
            label.text = info.upgradeName;
    }
}
