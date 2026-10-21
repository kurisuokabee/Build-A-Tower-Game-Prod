using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Enemy")]
    public GameObject prefab;
    public Sprite sprite;

    [Range(1, 3)]
    public int tier = 1;

    public EnemyMovement.MovementType movementType;
    public EnemyAttack.AttackType attackType;
}