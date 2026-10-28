using UnityEngine;
using System;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    [Header("Starting Resources")]
    [SerializeField] private int startingBlocks = 0;
    [SerializeField] private int startingExp = 0;

    [Header("Leveling")]
    [SerializeField] private int baseExpRequired = 100;
    [SerializeField] private float expGrowth = 1.25f;

    public int Blocks { get; private set; }
    public int Exp { get; private set; }
    public int Level { get; private set; } = 1;

    public int CurrentLevelExp { get; private set; }
    public int ExpToNextLevel { get; private set; }

    public int UpgradePoints { get; private set; }
    
    public event Action OnResourcesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Blocks = startingBlocks;
        Exp = startingExp;

        UpdateExpRequirement();
    }

    public void AddBlocks(int amount)
    {
        if (amount <= 0) return;

        Blocks += amount;
        OnResourcesChanged?.Invoke();
    }

    public void AddExp(int amount)
    {
        if (amount <= 0) return;

        Exp += amount;
        CurrentLevelExp += amount;

        while (CurrentLevelExp >= ExpToNextLevel)
        {
            CurrentLevelExp -= ExpToNextLevel;
            Level++;

            UpdateExpRequirement();

            Debug.Log($"Level Up! Level {Level}");
        }

        OnResourcesChanged?.Invoke();
    }

    private void UpdateExpRequirement()
    {
        ExpToNextLevel = Mathf.Max(
            1,
            Mathf.RoundToInt(
                baseExpRequired * Mathf.Pow(expGrowth, Level - 1)
            )
        );
    }

    public bool SpendBlocks(int amount)
    {
        if (amount < 0 || Blocks < amount)
            return false;

        Blocks -= amount;
        OnResourcesChanged?.Invoke();

        return true;
    }

    public bool SpendExp(int amount)
    {
        if (amount < 0 || Exp < amount)
            return false;

        Exp -= amount;
        OnResourcesChanged?.Invoke();

        return true;
    }

    public void AddUpgradePoints(int amount)
    {
        if (amount <= 0) return;
        UpgradePoints += amount;
        OnResourcesChanged?.Invoke();
    }
}