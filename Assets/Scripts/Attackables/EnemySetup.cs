
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void Initialize(
        EnemyAttack.AttackType attackType,
        TowerSection target,
        Sprite sprite)
    {
        if (enemyAttack == null)
            enemyAttack = GetComponent<EnemyAttack>();

        if (enemyMovement == null)
            enemyMovement = GetComponent<EnemyMovement>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        float attackRange = attackType == EnemyAttack.AttackType.Melee
            ? Random.Range(1.2f, 1.6f)
            : Random.Range(4.5f, 7f);

        if (enemyAttack != null)
            enemyAttack.Initialize(attackType, target, attackRange);

        if (enemyMovement != null)
            enemyMovement.Initialize(target, attackRange);

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.red;
        }
    }
}
