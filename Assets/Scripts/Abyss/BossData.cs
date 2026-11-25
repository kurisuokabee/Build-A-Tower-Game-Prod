
using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Data", menuName = "Abyss/Boss Data")]
public class BossData : ScriptableObject
{
    [Header("Boss Identity")]
    public string bossName = "Abyss Boss";
    public GameObject bossPrefab;
    public Sprite bossSprite;

    [Header("Event Settings")]
    public float eventTimerLength = 60f;

    [Header("Combat Stats")]
    [SerializeField] private float maxHealth = 300f;

    [Header("Rewards")]
    [Min(0)] public int blocksReward = 100;
    [Min(0)] public int expReward = 100;
    [Min(0)] public int upgradePointsReward = 1;

    public float MaxHealth => maxHealth;
}
