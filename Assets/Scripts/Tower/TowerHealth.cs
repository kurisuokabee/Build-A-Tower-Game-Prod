using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float baseMaxHealth = 100f;
    [SerializeField] private float healthPerSection = 50f;

    [Header("UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;

    private float currentHealth;
    private float maxHealth;
    private int sectionCount;
    private bool initialized;

    private void Awake()
    {
        maxHealth = baseMaxHealth;
        currentHealth = maxHealth;
        initialized = true;

        UpdateUI();
    }

    public void UpdateSectionCount(int newCount)
    {
        float previousMaxHealth = maxHealth;

        sectionCount = newCount;
        maxHealth = baseMaxHealth + sectionCount * healthPerSection;

        if (initialized)
        {
            // Only increase current HP by the amount of new max HP.
            currentHealth += maxHealth - previousMaxHealth;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }
        else
        {
            currentHealth = maxHealth;
            initialized = true;
        }

        UpdateUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0f, currentHealth - damage);
        UpdateUI();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        if (hpText != null)
        {
            hpText.text =
                $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }
}
