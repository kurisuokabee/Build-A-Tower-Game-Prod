
using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerSectionUpgradeTree : MonoBehaviour
{
    public enum UpgradeChoice
    {
        None,
        Left,
        Right,
        Left1,
        Left2,
        Right1,
        Right2
    }

    [Serializable]
    public class UpgradeInfo
    {
        public UpgradeChoice choice;

        [Header("Upgrade Information")]
        public string upgradeName;
        [TextArea] public string description;

        [Header("Upgrade Cost")]
        [Min(0)] public int upgradePointCost = 1;
    }

    [Header("Upgrade Names")]
    [SerializeField] private List<UpgradeInfo> upgrades = new();

    [Header("Upgrade State")]
    [SerializeField] private UpgradeChoice selectedBranch =
        UpgradeChoice.None;

    [SerializeField] private UpgradeChoice selectedSpecialization =
        UpgradeChoice.None;

    public bool HasChosenBranch =>
        selectedBranch != UpgradeChoice.None;

    public bool HasChosenSpecialization =>
        selectedSpecialization != UpgradeChoice.None;

    public UpgradeChoice SelectedBranch => selectedBranch;
    public UpgradeChoice SelectedSpecialization => selectedSpecialization;

    private void OnEnable()
    {
        TowerBuilding.Instance?.RefreshTemporaryUpgrades();
    }

    public string GetUpgradeName(UpgradeChoice choice)
    {
        UpgradeInfo info = upgrades.Find(x => x.choice == choice);

        return info != null &&
               !string.IsNullOrWhiteSpace(info.upgradeName)
            ? info.upgradeName
            : choice.ToString();
    }

    public string GetUpgradeDescription(UpgradeChoice choice)
    {
        UpgradeInfo info = upgrades.Find(x => x.choice == choice);

        return info != null ? info.description : "";
    }

    public int GetUpgradeCost(UpgradeChoice choice)
    {
        UpgradeInfo info = upgrades.Find(x => x.choice == choice);

        return info != null ? info.upgradePointCost : 0;
    }

    public bool CanAffordUpgrade(UpgradeChoice choice)
    {
        PlayerEconomy economy = PlayerEconomy.Instance;

        if (economy == null)
            return false;

        return economy.UpgradePoints >= GetUpgradeCost(choice);
    }

    public List<UpgradeChoice> GetAvailableChoices()
    {
        List<UpgradeChoice> choices = new();

        if (!HasChosenBranch)
        {
            choices.Add(UpgradeChoice.Left);
            choices.Add(UpgradeChoice.Right);
        }
        else if (!HasChosenSpecialization)
        {
            if (selectedBranch == UpgradeChoice.Left)
            {
                choices.Add(UpgradeChoice.Left1);
                choices.Add(UpgradeChoice.Left2);
            }
            else if (selectedBranch == UpgradeChoice.Right)
            {
                choices.Add(UpgradeChoice.Right1);
                choices.Add(UpgradeChoice.Right2);
            }
        }

        return choices;
    }

    public bool ChooseUpgrade(UpgradeChoice choice)
    {
        if (!GetAvailableChoices().Contains(choice))
        {
            Debug.LogWarning(
                $"[{name}] Cannot choose {choice}. " +
                "This upgrade is unavailable or already chosen."
            );

            return false;
        }

        int cost = GetUpgradeCost(choice);

        PlayerEconomy economy = PlayerEconomy.Instance;

        if (economy == null)
        {
            Debug.LogWarning(
                $"[{name}] Cannot purchase upgrade. PlayerEconomy is missing."
            );

            return false;
        }

        if (!economy.SpendUpgradePoints(cost))
        {
            Debug.Log(
                $"[{name}] Not enough Upgrade Points for {GetUpgradeName(choice)}. " +
                $"Required: {cost}, Available: {economy.UpgradePoints}."
            );

            return false;
        }

        if (!HasChosenBranch)
        {
            selectedBranch = choice;
        }
        else
        {
            selectedSpecialization = choice;
        }

        TowerBuilding.Instance?.RefreshTemporaryUpgrades();

        Debug.Log(
            $"[{name}] Purchased {GetUpgradeName(choice)} " +
            $"for {cost} Upgrade Points."
        );

        return true;
    }
}
