
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

    private TowerSectionUpgradeTree currentTree;

    private void Awake()
    {
        if (firstButton != null)
            firstButton.onClick.AddListener(OnFirstButtonClicked);

        if (secondButton != null)
            secondButton.onClick.AddListener(OnSecondButtonClicked);
    }

    private void OnDestroy()
    {
        if (firstButton != null)
            firstButton.onClick.RemoveListener(OnFirstButtonClicked);

        if (secondButton != null)
            secondButton.onClick.RemoveListener(OnSecondButtonClicked);
    }

    public void ShowUpgrades(TowerSection section)
    {
        currentTree = section != null
            ? section.GetComponent<TowerSectionUpgradeTree>()
            : null;

        if (upgradePanel != null)
            upgradePanel.SetActive(currentTree != null);

        if (currentTree != null)
            RefreshButtons();
    }

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
        if (currentTree == null)
            return;

        List<TowerSectionUpgradeTree.UpgradeChoice> choices =
            currentTree.GetAvailableChoices();

        if (buttonIndex >= choices.Count)
            return;

        if (currentTree.ChooseUpgrade(choices[buttonIndex]))
            RefreshButtons();
    }

    private void RefreshButtons()
    {
        if (currentTree == null)
            return;

        List<TowerSectionUpgradeTree.UpgradeChoice> choices =
            currentTree.GetAvailableChoices();

        SetButton(firstButton, firstButtonText, choices, 0);
        SetButton(secondButton, secondButtonText, choices, 1);
    }

    private void SetButton(
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
}
