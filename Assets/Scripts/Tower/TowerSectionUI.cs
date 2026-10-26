
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSectionUI : MonoBehaviour
{
    [Header("Section")]
    [SerializeField] private TowerSection sectionPrefab;
    [SerializeField] private TowerBuilding tower;
    [SerializeField] private TowerUpgradeID upgradeID;

    [Header("Build UI")]
    [SerializeField] private Button sectionButton;
    [SerializeField] private TMP_Text sectionNameText;
    [SerializeField] private TMP_Text blockCostText;
    [SerializeField] private TMP_Text levelText;

    [Header("Upgrade UI")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text resourceCostText;

    public static bool IsHoldingSection { get; private set; }
    public static bool IgnoreRelocatorClick { get; private set; }

    private PlayerEconomy economy;
    private TowerSectionUpgrade upgradeManager;

    private void Start()
    {
        economy = PlayerEconomy.Instance;
        upgradeManager = TowerSectionUpgrade.Instance;

        if (sectionButton != null)
            sectionButton.onClick.AddListener(OnSectionButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        SubscribeToEvents();
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (sectionButton != null)
            sectionButton.onClick.RemoveListener(OnSectionButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeClicked);

        if (economy != null)
            economy.OnResourcesChanged -= UpdateUI;

        if (upgradeManager != null)
            upgradeManager.OnUpgradesChanged -= UpdateUI;
    }

    private void Update()
    {
        if (economy != PlayerEconomy.Instance)
        {
            if (economy != null)
                economy.OnResourcesChanged -= UpdateUI;

            economy = PlayerEconomy.Instance;

            if (economy != null)
                economy.OnResourcesChanged += UpdateUI;

            UpdateUI();
        }

        if (upgradeManager != TowerSectionUpgrade.Instance)
        {
            if (upgradeManager != null)
                upgradeManager.OnUpgradesChanged -= UpdateUI;

            upgradeManager = TowerSectionUpgrade.Instance;

            if (upgradeManager != null)
                upgradeManager.OnUpgradesChanged += UpdateUI;

            UpdateUI();
        }
    }

    private void SubscribeToEvents()
    {
        if (economy != null)
            economy.OnResourcesChanged += UpdateUI;

        if (upgradeManager != null)
            upgradeManager.OnUpgradesChanged += UpdateUI;
    }

    public void UpdateUI()
    {
        if (sectionPrefab != null)
        {   
            int cost = tower != null
                ? tower.GetCurrentBuildCost(sectionPrefab)
                : sectionPrefab.BlockCost;

            if (sectionNameText != null)
                sectionNameText.text = sectionPrefab.name;

            if (blockCostText != null)
                blockCostText.text = $"{cost} Blocks";

            if (levelText != null)
            {
                int level = upgradeManager != null
                    ? upgradeManager.GetLevel(upgradeID)
                    : 0;

                levelText.text = $"Level: {level}";
            }

            if (sectionButton != null)
            {
                 sectionButton.interactable =
                    sectionPrefab.IsUnlocked &&
                    tower != null &&
                    PlayerEconomy.Instance != null &&
                    PlayerEconomy.Instance.Blocks >= cost;
            }
        }

        if (upgradeManager != null)
        {
            int expCost = upgradeManager.GetExpCost(upgradeID);

            if (resourceCostText != null)
                resourceCostText.text = $"{expCost} EXP";

            if (upgradeButton != null)
            {
                bool hasSection =
                    TowerBuilding.Instance != null &&
                    TowerBuilding.Instance.HasSection(upgradeID);

                upgradeButton.interactable =
                    hasSection &&
                    upgradeManager.CanUpgrade(upgradeID);
            }
        }
        else if (upgradeButton != null)
        {
            upgradeButton.interactable = false;
        }
    }

    private void OnSectionButtonClicked()
    {
        if (sectionPrefab == null || tower == null || economy == null)
            return;

        if (!sectionPrefab.IsUnlocked)
            return;

        if (!sectionPrefab.CanAfford)
        {
            Debug.Log("Not enough Blocks!");
            return;
        }

        TowerSection newSection = Instantiate(sectionPrefab);

        if (!tower.BuildTowerSection(newSection))
        {
            Destroy(newSection.gameObject);
            return;
        }

        UpdateUI();
    }

    private void OnUpgradeClicked()
    {
        if (upgradeManager == null)
            return;

        upgradeManager.Upgrade(upgradeID);
        UpdateUI();
    }

    public static void ClearRelocatorClick()
    {
        IgnoreRelocatorClick = false;
    }
}
