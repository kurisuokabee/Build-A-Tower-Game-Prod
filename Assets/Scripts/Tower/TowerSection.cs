using System.Collections.Generic;
using UnityEngine;

public abstract class TowerSection : MonoBehaviour
{
    [Header("Section Size")]
    public float height;
    public float width;

    [Header("Section Data")]
    [SerializeField] private TowerSectionData sectionData;
    [SerializeField] private TowerEnemyFinder enemyFinder;

    public TowerSectionData Data => sectionData;

    public float AttackRange { get; protected set; }
    public float AttacksPerSecond { get; protected set; }
    public float Damage { get; protected set; }

    public int RequiredLevel => sectionData != null ? sectionData.requiredLevel : 1;

    public bool IsUnlocked =>
        PlayerEconomy.Instance != null &&
        PlayerEconomy.Instance.Level >= RequiredLevel;

    public bool CanAfford =>
        PlayerEconomy.Instance != null &&
        PlayerEconomy.Instance.Blocks >= BlockCost;

    private float attackTimer;

    protected virtual void Start()
    {
        ApplyGlobalUpgrades();
    }

    public void ApplyGlobalUpgrades()
    {
        if (TowerSectionUpgrade.Instance == null)
            return;
        
        if (sectionData == null)
            return;

        ApplyUpgrade();
    }


    protected virtual void Awake()
    {
        if (enemyFinder == null)
            enemyFinder = GetComponent<TowerEnemyFinder>();

        if (sectionData == null)
            return;

        AttackRange = sectionData.attackRange;
        AttacksPerSecond = sectionData.attacksPerSecond;
        Damage = sectionData.damage;
    }

    protected virtual void Update()
    {
        if (enemyFinder == null || AttacksPerSecond <= 0f)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        List<Enemy> targets = FindTargets();

        if (targets.Count == 0)
            return;

        Attack(targets);
        attackTimer = 1f / AttacksPerSecond;
    }

    protected List<Enemy> FindNearestEnemies(int count)
    {
        return enemyFinder.FindNearestEnemies(AttackRange, count);
    }

    protected Enemy FindNearestEnemy()
    {
        return enemyFinder.FindNearestEnemy(AttackRange);
    }

    public int BlockCost
    {
        get
        {
            if (sectionData == null)
                return 0;

            if (TowerSectionUpgrade.Instance == null)
                return sectionData.cost;

            return TowerSectionUpgrade.Instance.GetBuildCost(
                sectionData.towerUpgradeID,
                sectionData.cost
            );
        }
    }

    protected abstract List<Enemy> FindTargets();

    protected abstract void Attack(List<Enemy> targets);

    public abstract void ApplyUpgrade();
}
