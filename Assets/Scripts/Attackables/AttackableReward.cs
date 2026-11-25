
using UnityEngine;

public class AttackableReward : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField, Min(0)] private int blocksReward = 5;
    [SerializeField, Min(0)] private int expReward = 10;
    [SerializeField, Min(0)] private int guaranteedUpgradePoints;
    [SerializeField, Range(0f, 1f)] private float upgradePointChance;

    [Header("Level Bonus")]
    [SerializeField, Min(0)] private int blocksPerLevel = 5;
    [SerializeField, Min(0)] private int expPerLevel = 5;

    [Header("Reward Popup")]
    [SerializeField] private Color blocksColor = Color.orange;
    [SerializeField] private Color expColor = Color.green;
    [SerializeField] private Color upgradePointColor = Color.red;

    private PlayerEconomy playerEconomy;
    private TowerUtilities towerUtilities;
    private bool hasRewarded;

    private bool useDebrisReward;
    private DebrisData.DebrisType debrisType;
    private int debrisRewardAmount;

    private float nextRewardMultiplier = 1f;

    private void Awake()
    {
        playerEconomy = PlayerEconomy.Instance;
        towerUtilities = TowerUtilities.Instance;
    }

    private void OnEnable()
    {
        hasRewarded = false;
        nextRewardMultiplier = 1f;
    }

    public void Initialize(int exp, int blocks, float upChance)
    {
        useDebrisReward = false;

        expReward = Mathf.Max(0, exp);
        blocksReward = Mathf.Max(0, blocks);
        guaranteedUpgradePoints = 0;
        upgradePointChance = Mathf.Clamp01(upChance);
    }

    public void Initialize(int exp, int blocks, int upgradePoints)
    {
        useDebrisReward = false;

        expReward = Mathf.Max(0, exp);
        blocksReward = Mathf.Max(0, blocks);
        guaranteedUpgradePoints = Mathf.Max(0, upgradePoints);
        upgradePointChance = 0f;
    }

    public void Initialize(DebrisData.DebrisType type, int amount)
    {
        useDebrisReward = true;

        debrisType = type;
        debrisRewardAmount = Mathf.Max(0, amount);
    }

    public void SetNextRewardMultiplier(float multiplier)
    {
        if (hasRewarded)
            return;

        nextRewardMultiplier = Mathf.Max(1f, multiplier);
    }

    public void GiveReward()
    {
        if (hasRewarded)
            return;

        hasRewarded = true;

        if (playerEconomy == null)
            playerEconomy = PlayerEconomy.Instance;

        if (towerUtilities == null)
            towerUtilities = TowerUtilities.Instance;

        if (playerEconomy == null)
        {
            Debug.LogWarning("PlayerEconomy is missing!");
            return;
        }

        if (useDebrisReward)
        {
            GiveDebrisReward();
            return;
        }

        GiveNormalReward();
        nextRewardMultiplier = 1f;
    }

    private void GiveDebrisReward()
    {
        int playerLevel = playerEconomy.Level;

        bool zealActive =
            towerUtilities != null &&
            towerUtilities.IsZealActive;

        switch (debrisType)
        {
            case DebrisData.DebrisType.Blocks:
            {
                int amount = debrisRewardAmount +
                    (blocksPerLevel * playerLevel);

                if (zealActive)
                    amount *= 2;

                playerEconomy.AddBlocks(amount);
                ShowRewardPopup(amount, "Blocks", blocksColor);
                break;
            }

            case DebrisData.DebrisType.Experience:
            {
                int amount = debrisRewardAmount +
                    (expPerLevel * playerLevel);

                if (zealActive)
                    amount *= 2;

                playerEconomy.AddExp(amount);
                ShowRewardPopup(amount, "EXP", expColor);
                break;
            }

            case DebrisData.DebrisType.UpgradePoint:
            {
                playerEconomy.AddUpgradePoints(debrisRewardAmount);
                ShowRewardPopup(debrisRewardAmount, "Gems", upgradePointColor);
                break;
            }
        }
    }

    private void GiveNormalReward()
    {
        int playerLevel = playerEconomy.Level;

        int finalBlocks = blocksReward + (blocksPerLevel * playerLevel);
        int finalExp = expReward + (expPerLevel * playerLevel);

        finalBlocks = Mathf.RoundToInt(finalBlocks * nextRewardMultiplier);
        finalExp = Mathf.RoundToInt(finalExp * nextRewardMultiplier);

        float finalUpgradePointChance = Mathf.Clamp01(
            upgradePointChance * nextRewardMultiplier
        );

        if (towerUtilities != null && towerUtilities.IsZealActive)
        {
            finalBlocks *= 2;
            finalExp *= 2;
        }

        playerEconomy.AddBlocks(finalBlocks);
        playerEconomy.AddExp(finalExp);

        ShowRewardPopup(finalBlocks, "Blocks", blocksColor);
        ShowRewardPopup(finalExp, "EXP", expColor);

        if (guaranteedUpgradePoints > 0)
        {
            playerEconomy.AddUpgradePoints(guaranteedUpgradePoints);
            ShowRewardPopup(
                guaranteedUpgradePoints,
                "Gems",
                upgradePointColor
            );
        }

        if (finalUpgradePointChance > 0f &&
            Random.value < finalUpgradePointChance)
        {
            playerEconomy.AddUpgradePoints(1);
            ShowRewardPopup(1, "Gems", upgradePointColor);

            Debug.Log("Upgrade Point dropped!");
        }
    }

    private void ShowRewardPopup(int amount, string rewardName, Color color)
    {
        if (amount <= 0 || RewardPopupManager.Instance == null)
            return;

        RewardPopupManager.Instance.ShowReward(
            transform.position,
            $"+{amount} {rewardName}",
            color
        );
    }
}
