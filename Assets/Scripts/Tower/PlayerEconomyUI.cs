using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEconomyUI : MonoBehaviour
{
    [Header("Resource Text")]
    [SerializeField] private TMP_Text blocksText;
    [SerializeField] private TMP_Text expText;
    [SerializeField] private TMP_Text uPointsText;

    [Header("Blocks UI")]
    [SerializeField] private Slider blocksSlider;
    [SerializeField] private TMP_Text blocksProgressText;
  

    [Header("Level UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private TMP_Text expProgressText;
    [SerializeField] private TMP_Text levelText;

    private PlayerEconomy economy;

    private void Start()
    {
        economy = PlayerEconomy.Instance;

        if (economy == null)
        {
            Debug.LogError("PlayerEconomy instance not found!");
            return;
        }

        economy.OnResourcesChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDestroy()
    {
        if (economy != null)
            economy.OnResourcesChanged -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (economy == null) return;

        if (blocksText != null)
            blocksText.text = $"Blocks: {economy.Blocks}";

        if (blocksSlider != null)
        {
            float maxBlocks = economy.MaxBlocks;

            blocksSlider.maxValue = maxBlocks;
            blocksSlider.value = Mathf.Clamp(economy.Blocks, 0, maxBlocks);

            if (blocksProgressText != null)
                blocksProgressText.text =
                    $"{economy.Blocks} / {maxBlocks}";
        }

        if (expText != null)
            expText.text = $"EXP: {economy.Exp}";

        if (expSlider != null)
        {
            expSlider.maxValue = economy.ExpToNextLevel;
            expSlider.value = economy.CurrentLevelExp;
        }

        if (expProgressText != null)
            expProgressText.text =
                $"{economy.CurrentLevelExp} / {economy.ExpToNextLevel}";

        if (levelText != null)
            levelText.text = $"Level {economy.Level}";

        if (uPointsText != null)
            uPointsText.text = $"UPoints: {economy.UpgradePoints}";
    }
}