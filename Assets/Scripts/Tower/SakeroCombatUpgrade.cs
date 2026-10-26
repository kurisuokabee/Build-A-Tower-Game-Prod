
using System;
using UnityEngine;

public class SakeroCombatUpgrade : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SakeroCombat sakeroCombat;

    [Header("Upgrade Increases")]
    [SerializeField] private float damagePerUpgrade = 1f;
    [SerializeField] private float attacksPerSecondPerUpgrade = 0.1f;
    [SerializeField] private float aoeRadiusPerUpgrade = 1f;

    [Header("Upgrade Costs")]
    [SerializeField] private int baseBlockCost = 100;
    [SerializeField] private int baseExpCost = 20;
    [SerializeField] private float blockCostMultiplier = 1.5f;
    [SerializeField] private float expCostMultiplier = 1.5f;

    public int UpgradeLevel { get; private set; }

    public static event Action OnUpgradesChanged;

    private void Awake()
    {
        if (sakeroCombat == null)
            sakeroCombat = GetComponent<SakeroCombat>();
    }

    public int GetBlockCost()
    {
        return Mathf.RoundToInt(
            baseBlockCost * Mathf.Pow(blockCostMultiplier, UpgradeLevel)
        );
    }

    public int GetExpCost()
    {
        return Mathf.RoundToInt(
            baseExpCost * Mathf.Pow(expCostMultiplier, UpgradeLevel)
        );
    }

    public bool CanUpgrade()
    {
        PlayerEconomy economy = PlayerEconomy.Instance;

        return sakeroCombat != null &&
               economy != null &&
               economy.Blocks >= GetBlockCost() &&
               economy.Exp >= GetExpCost();
    }

    public bool TryUpgrade()
    {
        PlayerEconomy economy = PlayerEconomy.Instance;

        if (economy == null || sakeroCombat == null || !CanUpgrade())
            return false;

        int blockCost = GetBlockCost();
        int expCost = GetExpCost();

        if (!economy.SpendBlocks(blockCost))
            return false;

        if (!economy.SpendExp(expCost))
        {
            economy.AddBlocks(blockCost);
            return false;
        }

        sakeroCombat.ApplyUpgrade(
            damagePerUpgrade,
            attacksPerSecondPerUpgrade,
            aoeRadiusPerUpgrade
        );

        UpgradeLevel++;

        OnUpgradesChanged?.Invoke();

        return true;
    }
}
