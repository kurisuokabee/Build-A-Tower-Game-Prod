
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyMovement enemyMovement;
    [SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] private SpriteRenderer spriteRenderer;

    float attackRange;
    public void Initialize(
        EnemyMovement.MovementType movementType,
        EnemyAttack.AttackType attackType,
        TowerSection target,
        Sprite sprite)
    {
        if (enemyMovement == null)
            enemyMovement = GetComponent<EnemyMovement>();

        if (enemyAttack == null)
            enemyAttack = GetComponent<EnemyAttack>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackType == EnemyAttack.AttackType.Melee)
            attackRange = Random.Range(1.2f, 1.6f);
        else
            attackRange = Random.Range(4.5f, 7);

        if (enemyMovement != null)
        {
            enemyMovement.Initialize(movementType, target, attackRange);
        }

        if (enemyAttack != null)
        {
            enemyAttack.Initialize(attackType, target, attackRange);
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.red;
        }
            
    }

    
}
