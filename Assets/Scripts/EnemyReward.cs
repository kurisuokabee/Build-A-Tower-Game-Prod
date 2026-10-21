using UnityEngine;

public class EnemyReward : MonoBehaviour
{
    [Header("Rewards")]
    [SerializeField] private int blocksReward = 5;
    [SerializeField] private int expReward = 10;

    private bool hasRewarded;

    public void GiveReward()
    {
        if (hasRewarded) return;

        hasRewarded = true;

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("PlayerEconomy is missing!");
            return;
        }

        PlayerEconomy.Instance.AddBlocks(blocksReward);
        PlayerEconomy.Instance.AddExp(expReward);
    }

    public void ResetReward()
    {
        hasRewarded = false;
    }
}