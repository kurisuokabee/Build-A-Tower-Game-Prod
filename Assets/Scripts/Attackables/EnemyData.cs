using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy")]
    public GameObject prefab;
    public Sprite sprite;

    public EnemyAttack.AttackType attackType;
}