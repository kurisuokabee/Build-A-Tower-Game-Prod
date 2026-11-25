
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AttackableReward attackableReward;

    private void Awake()
    {
        if (enemyAttack == null)
            enemyAttack = GetComponent<EnemyAttack>();

        if (enemyMovement == null)
            enemyMovement = GetComponent<EnemyMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackableReward == null)
            attackableReward = GetComponent<AttackableReward>();
    }

    public void Initialize(EnemyData data, TowerSection target)
    {
        if (data == null)
            return;

        float attackRange = data.attackType == EnemyAttack.AttackType.Melee
            ? Random.Range(1.2f, 1.6f)
            : Random.Range(4.5f, 7f);

        if (enemyAttack != null)
            enemyAttack.Initialize(data.attackType, target);

        if (enemyMovement != null)
            enemyMovement.Initialize(target, attackRange);

        if (spriteRenderer != null)
            spriteRenderer.sprite = data.sprite;

        if (attackableReward != null)
        {
            attackableReward.Initialize(
                data.expReward,
                data.blocksReward,
                data.upgradePointsReward
            );
        }
    }
}
