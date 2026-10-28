using UnityEngine;

[CreateAssetMenu(fileName = "New Debris Data", menuName = "Debris/Debris Data")]
public class DebrisData : ScriptableObject
{
    public enum DebrisSize { Small, Medium, Large }

    [Header("Identity & Visuals")]
    public DebrisSize size;
    public GameObject prefab;
    public Sprite sprite;

    [Header("Combat Stats")]
    [SerializeField] private float maxHealth = 10f;
    [Min(0f)] [SerializeField] private float minFallSpeed = 2f;
    [Min(0f)] [SerializeField] private float maxFallSpeed = 5f;

    [Header("Rewards")]
    [SerializeField] private int baseExp = 2;
    [SerializeField] private int baseBlocks = 2;
    [Range(0f, 1f)] [SerializeField] private float upgradePointChance = 0.01f;

    public float MaxHealth => maxHealth;
    public float MinFallSpeed => minFallSpeed;
    public float MaxFallSpeed => maxFallSpeed;
    public int BaseExp => baseExp;
    public int BaseBlocks => baseBlocks;
    public float UpgradePointChance => upgradePointChance;
}