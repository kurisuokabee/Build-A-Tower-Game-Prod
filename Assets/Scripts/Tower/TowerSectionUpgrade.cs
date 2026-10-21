
using System;
using System.Collections.Generic;
using UnityEngine;

public class TowerSectionUpgrade : MonoBehaviour
{
    public static TowerSectionUpgrade Instance { get; private set; }

    [Serializable]
    public class UpgradeData
    {
        public TowerUpgradeID sectionId;

        [Header("Build Cost")]
        public int buildCostIncreasePerUpgrade = 10;

        [Header("Block Cost")]
        public int baseUpgradeCost = 100;
        public float blockCostMultiplier = 1.5f;

        [Header("EXP Cost")]
        public int baseExpCost = 50;
        public float expCostMultiplier = 1.5f;

        [NonSerialized] public int level;
    }

    [SerializeField] private List<UpgradeData> upgrades = new();

    public event Action OnUpgradesChanged;

    private readonly Dictionary<TowerUpgradeID, UpgradeData> upgradeLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        foreach (UpgradeData data in upgrades)
        {
            upgradeLookup[data.sectionId] = data;
        }
    }

    public int GetLevel(TowerUpgradeID sectionId)
    {
        return upgradeLookup.TryGetValue(sectionId, out var data)
            ? data.level
            : 0;
    }

    public int GetBlockCost(TowerUpgradeID sectionId)
    {
        if (!upgradeLookup.TryGetValue(sectionId, out var data))
            return -1;

        return Mathf.RoundToInt(
            data.baseUpgradeCost *
            Mathf.Pow(data.blockCostMultiplier, data.level));
    }

    public int GetExpCost(TowerUpgradeID sectionId)
    {
        if (!upgradeLookup.TryGetValue(sectionId, out var data))
            return -1;

        return Mathf.RoundToInt(
            data.baseExpCost *
            Mathf.Pow(data.expCostMultiplier, data.level));
    }

    public bool CanUpgrade(TowerUpgradeID sectionId)
    {
        if (!upgradeLookup.ContainsKey(sectionId) ||
            PlayerEconomy.Instance == null)
            return false;

        return PlayerEconomy.Instance.Blocks >= GetBlockCost(sectionId)
            && PlayerEconomy.Instance.Exp >= GetExpCost(sectionId);
    }

    public bool Upgrade(TowerUpgradeID sectionId)
    {
        if (!CanUpgrade(sectionId))
            return false;

        UpgradeData data = upgradeLookup[sectionId];

        int blockCost = GetBlockCost(sectionId);
        int expCost = GetExpCost(sectionId);

        PlayerEconomy economy = PlayerEconomy.Instance;

        if (!economy.SpendBlocks(blockCost))
            return false;

        if (!economy.SpendExp(expCost))
        {
            economy.AddBlocks(blockCost);
            return false;
        }

        data.level++;

        if (TowerBuilding.Instance != null)
        {
            foreach (TowerSection section in TowerBuilding.Instance.Sections)
            {
                if (section != null && section.Data != null &&
                    section.Data.towerUpgradeID == sectionId)
                {
                    section.ApplyUpgrade(data.level % 5 == 0);
                }
            }
        }

        OnUpgradesChanged?.Invoke();
        return true;
    }

    public int GetBuildCost(TowerUpgradeID sectionId, int baseCost)
    {
        if (!upgradeLookup.TryGetValue(sectionId, out var data))
            return baseCost;

        return baseCost +
            data.buildCostIncreasePerUpgrade * data.level;
    }
}
