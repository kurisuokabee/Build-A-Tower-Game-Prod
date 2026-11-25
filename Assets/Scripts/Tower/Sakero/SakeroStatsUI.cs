
using UnityEngine;
using TMPro;

public class SakeroStatsUI : MonoBehaviour
{
    [Header("Stats Text")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text attackSpeedText;

    private SakeroWand sakeroWand;
    private SakeroCombatUpgrade combatUpgrade;

    private float updateTimer;
    [SerializeField] private float updateInterval = 0.25f;

    private void OnEnable()
    {
        SakeroCombatUpgrade.OnUpgradesChanged += UpdateStats;
        FindSakeroReferences();
        UpdateStats();
    }

    private void OnDisable()
    {
        SakeroCombatUpgrade.OnUpgradesChanged -= UpdateStats;
    }

    private void Update()
    {
        updateTimer += Time.deltaTime;

        if (updateTimer < updateInterval)
            return;

        updateTimer = 0f;

        if (sakeroWand == null || combatUpgrade == null)
            FindSakeroReferences();

        UpdateStats();
    }

    private void FindSakeroReferences()
    {
        sakeroWand = FindAnyObjectByType<SakeroWand>();
        combatUpgrade = FindAnyObjectByType<SakeroCombatUpgrade>();
    }

    private void UpdateStats()
    {
        if (sakeroWand == null || combatUpgrade == null)
            return;

        if (levelText != null)
            levelText.text = $"LVL: {combatUpgrade.UpgradeLevel}";

        if (damageText != null)
            damageText.text = $"DMG: {sakeroWand.CurrentDamage:0.##}";

        if (attackSpeedText != null)
        {
            attackSpeedText.text =
            $"ATK SPD: {sakeroWand.CurrentAttacksPerSecond:0.##}";
        }
    }
}
