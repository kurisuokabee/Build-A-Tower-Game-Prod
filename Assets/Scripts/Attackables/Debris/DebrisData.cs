
using UnityEngine;

[CreateAssetMenu(fileName = "New Debris Data", menuName = "Debris/Debris Data")]
public class DebrisData : ScriptableObject
{
    public enum DebrisType
    {
        Blocks,
        Experience,
        UpgradePoint
    }

    [Header("Identity & Visuals")]
    public DebrisType type;
    public GameObject prefab;
    public Sprite sprite;

    [Header("Spawn Weight")]
    [Min(0f)] public float spawnWeight = 70f;

    [Header("Combat Stats")]
    [SerializeField] private float maxHealth = 10f;
    [Min(0f)] [SerializeField] private float minFallSpeed = 2f;
    [Min(0f)] [SerializeField] private float maxFallSpeed = 5f;

    [Header("Reward")]
    [Min(0)] [SerializeField] private int baseReward = 2;

    public float MaxHealth => maxHealth;
    public float MinFallSpeed => minFallSpeed;
    public float MaxFallSpeed => maxFallSpeed;
    public int BaseReward => baseReward;
}
