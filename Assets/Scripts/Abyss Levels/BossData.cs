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
    [SerializeField] private float damage = 25f;

    public float MaxHealth => maxHealth;
    public float Damage => damage;
}