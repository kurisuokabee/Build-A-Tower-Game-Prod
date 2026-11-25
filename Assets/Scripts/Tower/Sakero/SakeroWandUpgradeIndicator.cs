
using UnityEngine;

public class SakeroWandUpgradeIndicator : MonoBehaviour
{
    [Header("Indicator")]
    [SerializeField] private GameObject indicator;

    private PlayerEconomy playerEconomy;

    private void OnEnable()
    {
        playerEconomy = PlayerEconomy.Instance;

        if (playerEconomy != null)
            playerEconomy.OnResourcesChanged += RefreshIndicator;

        SakeroWandUpgradeTree.OnAnyUpgradeStateChanged += RefreshIndicator;

        RefreshIndicator();
    }

    private void OnDisable()
    {
        if (playerEconomy != null)
            playerEconomy.OnResourcesChanged -= RefreshIndicator;

        SakeroWandUpgradeTree.OnAnyUpgradeStateChanged -= RefreshIndicator;
    }

    private void RefreshIndicator()
    {
        if (indicator == null)
            return;

        SakeroWandUpgradeTree[] upgradeTrees =
            FindObjectsByType<SakeroWandUpgradeTree>();

        bool hasAvailableUpgrade = false;

        foreach (SakeroWandUpgradeTree tree in upgradeTrees)
        {
            if (tree.CanAffordUpgrade(
                SakeroWandUpgradeTree.UpgradeChoice.Left) ||
                tree.CanAffordUpgrade(
                SakeroWandUpgradeTree.UpgradeChoice.Right))
            {
                hasAvailableUpgrade = true;
                break;
            }
        }

        indicator.SetActive(hasAvailableUpgrade);
    }
}
