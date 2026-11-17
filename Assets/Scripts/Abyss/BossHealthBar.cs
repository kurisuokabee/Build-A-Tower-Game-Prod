
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("Boss Health")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider ghostSlider;

    [Header("Shield Health")]
    [SerializeField] private Slider shieldSlider;

    [Header("Ghost Bar Settings")]
    [SerializeField, Min(0f)] private float damageDelay = 0.5f;
    [SerializeField, Min(0.1f)] private float ghostDrainSpeed = 0.5f;

    private AttackableHealth bossHealth;
    private BossShield bossShield;
    private Abyss3Boss abyss3Boss;

    private float damageTimer;

    private void Update()
    {
        UpdateBossHealth();
        UpdateShieldHealth();
    }

    private void UpdateBossHealth()
    {
        if (bossHealth == null)
            return;

        float healthPercentage = Mathf.Clamp01(
            bossHealth.CurrentHealth / bossHealth.MaxHealth
        );

        healthSlider.value = healthPercentage;

        if (healthPercentage > ghostSlider.value)
        {
            ghostSlider.value = healthPercentage;
            damageTimer = 0f;
            return;
        }

        if (ghostSlider.value > healthPercentage)
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= damageDelay)
            {
                ghostSlider.value = Mathf.MoveTowards(
                    ghostSlider.value,
                    healthPercentage,
                    ghostDrainSpeed * Time.deltaTime
                );
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    private void UpdateShieldHealth()
    {
        // Abyss 3: Minion kill progress
        if (abyss3Boss != null)
        {
            bool showShield =
                !abyss3Boss.IsVulnerable &&
                !abyss3Boss.IsDefeated;

            shieldSlider.gameObject.SetActive(showShield);

            if (showShield)
            {
                float progress = 1f -
                    ((float)abyss3Boss.MinionsKilled /
                    abyss3Boss.RequiredMinionKills);

                shieldSlider.value = Mathf.Clamp01(progress);
            }

            return;
        }

        // Abyss 1: Actual shield health
        if (bossShield == null)
        {
            shieldSlider.gameObject.SetActive(false);
            return;
        }

        bool shieldActive = bossShield.IsActive;

        shieldSlider.gameObject.SetActive(shieldActive);

        if (!shieldActive)
            return;

        AttackableHealth shieldHealth = bossShield.Health;

        if (shieldHealth == null)
            return;

        shieldSlider.value = Mathf.Clamp01(
            shieldHealth.CurrentHealth / shieldHealth.MaxHealth
        );
    }

    public void SetBoss(BossController boss)
    {
        if (boss == null || boss.Health == null)
            return;

        bossHealth = boss.Health;

        Abyss1Boss abyss1Boss = boss as Abyss1Boss;
        bossShield = abyss1Boss != null ? abyss1Boss.Shield : null;

        abyss3Boss = boss as Abyss3Boss;

        healthSlider.minValue = 0f;
        healthSlider.maxValue = 1f;

        ghostSlider.minValue = 0f;
        ghostSlider.maxValue = 1f;

        shieldSlider.minValue = 0f;
        shieldSlider.maxValue = 1f;
        shieldSlider.value = 1f;
        shieldSlider.gameObject.SetActive(false);

        float initialPercentage = Mathf.Clamp01(
            bossHealth.CurrentHealth / bossHealth.MaxHealth
        );

        healthSlider.value = initialPercentage;
        ghostSlider.value = initialPercentage;

        damageTimer = 0f;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        bossHealth = null;
        bossShield = null;
        abyss3Boss = null;

        shieldSlider.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
