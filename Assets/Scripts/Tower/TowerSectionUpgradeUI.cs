using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerSectionUpgradeUI : MonoBehaviour
{
    [Header("Section")]
    [SerializeField] private TowerUpgradeID upgradeID;

    [Header("UI References")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text sectionNameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text resourceCostText;
    [SerializeField] private TMP_Text specialUpgradeText;

    private TowerSectionUpgrade upgradeManager;
    private PlayerEconomy economy;

    private void Start()
    {
        upgradeManager = TowerSectionUpgrade.Instance;
        economy = PlayerEconomy.Instance;

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        if (upgradeManager != null)
            upgradeManager.OnUpgradesChanged += UpdateUI;

        if (economy != null)
            economy.OnResourcesChanged += UpdateUI;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

        if (upgradeManager != null)
            upgradeManager.OnUpgradesChanged -= UpdateUI;

        if (economy != null)
            economy.OnResourcesChanged -= UpdateUI;
    }

    private void OnUpgradeClicked()
    {
        if (upgradeManager == null)
            return;

        upgradeManager.Upgrade(upgradeID);
        UpdateUI();
    }

    private void UpdateUI()
    {   
        if (sectionNameText != null)
            sectionNameText.text = upgradeID.ToString();

        if (upgradeManager == null)
            upgradeManager = TowerSectionUpgrade.Instance;

        if (economy == null)
            economy = PlayerEconomy.Instance;

        if (upgradeManager == null)
            return;

        int level = upgradeManager.GetLevel(upgradeID);
        int blockCost = upgradeManager.GetBlockCost(upgradeID);
        int expCost = upgradeManager.GetExpCost(upgradeID);

        if (levelText != null)
            levelText.text = $"Level {level}";

        if (resourceCostText != null)
            resourceCostText.text = $"{blockCost} Blocks\n{expCost} EXP";
        

        if (specialUpgradeText != null)
        {
            bool isSpecial = (level + 1) % 5 == 0;
            specialUpgradeText.gameObject.SetActive(isSpecial);

            if (isSpecial)
                specialUpgradeText.text = "SPECIAL UPGRADE";
        }

        if (upgradeButton != null)
        {
            bool hasSection =
                TowerBuilding.Instance != null &&
                TowerBuilding.Instance.HasSection(upgradeID);

            upgradeButton.interactable =
                hasSection && upgradeManager.CanUpgrade(upgradeID);
        }
    }
}
