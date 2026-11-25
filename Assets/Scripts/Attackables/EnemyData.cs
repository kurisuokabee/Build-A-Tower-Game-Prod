
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy")]
    public GameObject prefab;
    public Sprite sprite;

    public EnemyAttack.AttackType attackType;

    [Header("Rewards")]
    [Min(0)] public int blocksReward = 5;
    [Min(0)] public int expReward = 10;
    [Min(0)] public int upgradePointsReward = 0;
}
