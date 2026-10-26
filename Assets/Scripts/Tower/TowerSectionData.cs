using UnityEngine;

[CreateAssetMenu(fileName = "New Tower Section", menuName = "Tower/Tower Section Data")]
public class TowerSectionData : ScriptableObject
{
    [Header("Section Info")]
    public string sectionName;
    public Sprite icon;
    public int cost = 100;
    public int requiredLevel = 1;
    public TowerUpgradeID towerUpgradeID;

    [Header("Attack Settings")]
    public float attackRange = 5f;
    public float attacksPerSecond = 1f;
    public float damage = 10f;

}

public enum TowerUpgradeID
{
    Arrow,
    Spear,
    Cannon,
    Flame
}
