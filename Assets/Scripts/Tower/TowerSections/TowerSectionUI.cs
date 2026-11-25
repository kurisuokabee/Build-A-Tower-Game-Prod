
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
    [SerializeField] private TMP_Text blockCostText;
    

    [Header("Upgrade UI")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text resourceCostText;

    [Header("Stats UI")]
    [SerializeField] private TMP_Text statsLevelText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text attackSpeedText;

    public static bool IsHoldingSection { get; private set; }
    public static bool IgnoreRelocatorClick { get; private set; }

    private PlayerEconomy economy;
    private TowerSectionUpgrade upgradeManager;

    private float updateTimer;

    [SerializeField] private float updateInterval = 1f;

    private void Start()
    {
        economy = PlayerEconomy.Instance;
        upgradeManager = TowerSectionUpgrade.Instance;

        if (sectionButton != null)
            sectionButton.onClick.AddListener(OnSectionButtonClicked);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);

        if (tower != null)
            tower.OnTowerSectionsChanged += UpdateUI;

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

        if (tower != null)
            tower.OnTowerSectionsChanged -= UpdateUI;
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer >= updateInterval)
        {
            updateTimer = 0f;
            UpdateUI();
        }

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

            if (blockCostText != null)
                blockCostText.text = $"{cost} Blocks";

            if (sectionButton != null)
            {
                sectionButton.interactable =
                    sectionPrefab.IsUnlocked &&
                    tower != null &&
                    tower.Sections.Count < AbyssManager.Instance.CurrentBuildLimit &&
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

        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        if (sectionPrefab == null || sectionPrefab.Data == null)
            return;

        int level = upgradeManager != null
            ? upgradeManager.GetLevel(upgradeID)
            : 0;

        float damage = sectionPrefab.Data.damage;
        float attackSpeed = sectionPrefab.Data.attacksPerSecond;

        // Use actual stats from a built section when available.
        if (tower != null && tower.Sections != null)
        {
            foreach (TowerSection section in tower.Sections)
            {
                if (section == null || section.Data == null)
                    continue;

                if (section.Data.towerUpgradeID != upgradeID)
                    continue;

                damage = section.EffectiveDamage;
                attackSpeed = section.EffectiveAttacksPerSecond;
                break;
            }
        }

        if (statsLevelText != null)
            statsLevelText.text = $"LVL: {level}";

        if (damageText != null)
            damageText.text = $"DMG: {damage:0.##}";

        if (attackSpeedText != null)
            attackSpeedText.text = $"ATK SPD: {attackSpeed:0.##}";
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
        
        newSection.PlayPlaceEffect();

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
