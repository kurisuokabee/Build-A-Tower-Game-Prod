using UnityEngine;

public class AttackableReward : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField, Min(0)] private int blocksReward = 5;
    [SerializeField, Min(0)] private int expReward = 10;
    [SerializeField, Range(0f, 1f)] private float upgradePointChance;

    [Header("Level Bonus")]
    [SerializeField, Min(0)] private int blocksPerLevel = 5;
    [SerializeField, Min(0)] private int expPerLevel = 5;

    private PlayerEconomy playerEconomy;
    private TowerUtilities towerUtilities;
    private bool hasRewarded;

    void Awake()
    {
        playerEconomy = PlayerEconomy.Instance;
        towerUtilities = TowerUtilities.Instance;
    }

    private void OnEnable()
    {
        hasRewarded = false;
    }

    public void Initialize(int exp, int blocks, float upChance)
    {
        expReward = Mathf.Max(0, exp);
        blocksReward = Mathf.Max(0, blocks);
        upgradePointChance = Mathf.Clamp01(upChance);
    }

    public void GiveReward()
    {
        if (hasRewarded)
            return;

        hasRewarded = true;

        if (playerEconomy == null)
        {
            Debug.LogWarning("PlayerEconomy is missing!");
            return;
        }

        int playerLevel = playerEconomy.Level;

        int finalBlocks = blocksReward + (blocksPerLevel * playerLevel);
        int finalExp = expReward + (expPerLevel * playerLevel);

        if (towerUtilities  != null &&
            towerUtilities .IsZealActive)
        {
            finalBlocks *= 2;
            finalExp *= 2;
        }

        playerEconomy.AddBlocks(finalBlocks);
        playerEconomy.AddExp(finalExp);

        if (upgradePointChance > 0f &&
            Random.value < upgradePointChance)
        {
            playerEconomy.AddUpgradePoints(1);
            Debug.Log("Upgrade Point dropped!");
        }
    }
}
