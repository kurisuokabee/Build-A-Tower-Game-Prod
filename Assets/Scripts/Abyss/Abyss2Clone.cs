
using UnityEngine;

public class Abyss2Clone : MonoBehaviour
{
    private Abyss2Boss bossController;
    private AttackableHealth ownHealth;
    private bool isOriginal;

    public bool IsMainBoss { get; private set; }

    public void Initialize(Abyss2Boss boss, bool isOriginal)
    {
        bossController = boss;
        this.isOriginal = isOriginal;

        ownHealth = GetComponent<AttackableHealth>();

        SetMainBoss(false);
    }

    public void SetMainBoss(bool isMainBoss)
    {
        IsMainBoss = isMainBoss;

        if (ownHealth == null)
            ownHealth = GetComponent<AttackableHealth>();

        if (!isOriginal && ownHealth != null)
            ownHealth.CanTakeDamage = isMainBoss;
    }

    public bool TryForwardDamage(float damage)
    {
        if (!IsMainBoss || bossController == null)
            return true;

        if (isOriginal)
            return false;

        bossController.SharedHealth.TakeSharedDamage(damage);
        return true;
    }
}
