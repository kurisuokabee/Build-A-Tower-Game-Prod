
using System;
using UnityEngine;

public class SakeroWandUpgradeTree : MonoBehaviour
{
    public enum UpgradeChoice
    {
        None,
        Left,
        Right
    }

    [Serializable]
    public class UpgradeInfo
    {
        public UpgradeChoice choice;
        public string upgradeName;
        [TextArea] public string description;
        [Min(0)] public int upgradePointCost = 1;
    }

    [Header("Upgrades")]
    [SerializeField] private UpgradeInfo leftUpgrade;
    [SerializeField] private UpgradeInfo rightUpgrade;

    private SakeroWand wand;
    public static event Action OnAnyUpgradeStateChanged;
    public UpgradeChoice SelectedUpgrade { get; private set; }

    private void Awake()
    {
        wand = GetComponent<SakeroWand>();
    }

    public UpgradeInfo GetUpgradeInfo(UpgradeChoice choice)
    {
        return choice switch
        {
            UpgradeChoice.Left => leftUpgrade,
            UpgradeChoice.Right => rightUpgrade,
            _ => null
        };
    }

    public bool CanAffordUpgrade(UpgradeChoice choice)
    {
        UpgradeInfo info = GetUpgradeInfo(choice);

        return SelectedUpgrade == UpgradeChoice.None &&
               info != null &&
               PlayerEconomy.Instance != null &&
               PlayerEconomy.Instance.UpgradePoints >= info.upgradePointCost;
    }

    public bool ChooseUpgrade(UpgradeChoice choice)
    {
        if (!CanAffordUpgrade(choice))
            return false;

        UpgradeInfo info = GetUpgradeInfo(choice);

        if (!PlayerEconomy.Instance.SpendUpgradePoints(info.upgradePointCost))
            return false;

        SelectedUpgrade = choice;

        ApplyUpgrade();
        OnAnyUpgradeStateChanged?.Invoke();
        Debug.Log($"{wand.name} selected {info.upgradeName}!");

        return true;
    }

    private void ApplyUpgrade()
    {
        if (wand is MeteorWand meteorWand)
        {
            meteorWand.SetLeftUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Left
            );

            meteorWand.SetRightUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Right
            );
        }

        if (wand is IceWand iceWand)
        {
            iceWand.SetLeftUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Left
            );

            iceWand.SetRightUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Right
            );
        }

        if (wand is ElectricWand electricWand)
        {
            electricWand.SetLeftUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Left
            );

            electricWand.SetRightUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Right
            );
        }

        if (wand is VortexWand vortexWand)
        {
            vortexWand.SetLeftUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Left
            );

            vortexWand.SetRightUpgradeEnabled(
                SelectedUpgrade == UpgradeChoice.Right
            );
        }
    }
}
