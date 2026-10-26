
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SakeroCombatUpgradeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SakeroCombatUpgrade upgrade;

    [Header("UI Elements")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text costText;

    private PlayerEconomy economy;

    private void Awake()
    {
        if (upgrade == null)
            upgrade = FindFirstObjectByType<SakeroCombatUpgrade>();

        if (upgradeButton == null)
            Debug.LogError("Sakero upgrade button is not assigned.", this);

        if (upgrade == null)
            Debug.LogError("SakeroCombatUpgrade was not found.", this);
    }

    private void OnEnable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);

        SakeroCombatUpgrade.OnUpgradesChanged += UpdateUI;

        ConnectEconomy();
        UpdateUI();
    }

    private void Update()
    {
        if (economy != PlayerEconomy.Instance)
        {
            DisconnectEconomy();
            ConnectEconomy();
            UpdateUI();
        }
    }

    private void OnDisable()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeButtonClicked);

        SakeroCombatUpgrade.OnUpgradesChanged -= UpdateUI;
        DisconnectEconomy();
    }

    private void ConnectEconomy()
    {
        economy = PlayerEconomy.Instance;

        if (economy != null)
            economy.OnResourcesChanged += UpdateUI;
    }

    private void DisconnectEconomy()
    {
        if (economy != null)
            economy.OnResourcesChanged -= UpdateUI;

        economy = null;
    }

    private void OnUpgradeButtonClicked()
    {
        if (upgrade == null)
            return;

        bool success = upgrade.TryUpgrade();

        if (!success)
            Debug.LogWarning(
                "Upgrade failed. Check Blocks, EXP, and SakeroCombat references.",
                this
            );

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (upgrade == null)
            return;

        if (levelText != null)
            levelText.text = "Lv. " + upgrade.UpgradeLevel;

        if (costText != null)
        {
            costText.text =
                upgrade.GetBlockCost() + " Blocks\n" +
                upgrade.GetExpCost() + " EXP";
        }

        if (upgradeButton != null)
            upgradeButton.interactable = upgrade.CanUpgrade();
    }
}
