
using UnityEngine;

public class EnemySetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyAttack enemyAttack;
    [SerializeField] private SpriteRenderer spriteRenderer;

    float attackRange;
    public void Initialize(
        EnemyAttack.AttackType attackType,
        TowerSection target,
        Sprite sprite)
    {

        if (enemyAttack == null)
            enemyAttack = GetComponent<EnemyAttack>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (attackType == EnemyAttack.AttackType.Melee)
            attackRange = Random.Range(1.2f, 1.6f);
        else
            attackRange = Random.Range(4.5f, 7);

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
