
using UnityEngine;

[RequireComponent(typeof(AttackableHealth))]
public class BossShield : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AttackableHealth health;
    [SerializeField] private Abyss1Boss boss;

    public AttackableHealth Health => health;

    private bool isActive;

    public bool IsActive => isActive;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<AttackableHealth>();

        if (boss == null)
            boss = GetComponentInParent<Abyss1Boss>();

        health.OnDied += HandleShieldDestroyed;
        health.ReturnToPoolOnDeath = false;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDied -= HandleShieldDestroyed;
    }

    private void OnEnable()
    {
        isActive = false;

        if (health != null)
        {
            health.CanTakeDamage = false;
            health.ResetHealth();
            health.CanTakeDamage = false;
        }
    }

    public void Activate()
    {
        if (isActive)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        isActive = true;

        health.ResetHealth();
        health.CanTakeDamage = true;

        Debug.Log("Boss shield activated. Can take damage: " + health.CanTakeDamage);
    }

    public void Deactivate()
    {
        isActive = false;

        if (health != null)
            health.CanTakeDamage = false;

        gameObject.SetActive(false);
    }

    private void HandleShieldDestroyed()
    {
        if (!isActive)
            return;

        isActive = false;
        health.CanTakeDamage = false;

        gameObject.SetActive(false);

        Debug.Log("Boss shield destroyed!");

        if (boss != null)
            boss.OnShieldDestroyed();
    }
}
