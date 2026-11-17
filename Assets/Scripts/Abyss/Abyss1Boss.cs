
using UnityEngine;

public class Abyss1Boss : BossController
{
    public enum BossState
    {
        Idle,
        Attacking,
        Stunned,
        Shielded,
        Defeated
    }

    private const float AttackDelay = 3f;

    [Header("Attack Settings")]
    [SerializeField, Min(0f)] private float stunDuration = 3f;

    [Header("References")]
    [SerializeField] private Abyss1AttackController attackController;
    [SerializeField] private BossShield shield;

    private float attackTimer;
    private float stunTimer;
    private bool hasHitTower;

    [SerializeField, Min(0f)] private float movementResumeDelay = 5f;

    private float movementResumeTimer;

    public bool CanMove { get; private set; } = true;

    public BossState CurrentState { get; private set; }

    public BossShield Shield => shield;

    protected override void Awake()
    {
        base.Awake();

        if (attackController == null)
            attackController = GetComponent<Abyss1AttackController>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        CurrentState = BossState.Idle;
        attackTimer = AttackDelay;
        stunTimer = 0f;
        hasHitTower = false;
    }

    

    private void Update()
    {
        if (IsDefeated || hasHitTower)
            return;

        if (!CanMove)
        {
            movementResumeTimer -= Time.deltaTime;

            if (movementResumeTimer <= 0f)
                CanMove = true;
        }

        if (CurrentState == BossState.Stunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0f)
                ActivateShield();

            return;
        }

        if (CurrentState != BossState.Idle)
            return;

        if (attackController == null || attackController.HasActiveProjectile)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
            StartAttack();
    }

    private void StartAttack()
    {
        if (CurrentState != BossState.Idle || IsDefeated)
            return;

        if (attackController.LaunchProjectile())
        {
            CurrentState = BossState.Attacking;
        }
        else
        {
            attackTimer = 1f;
        }
    }

    public void OnProjectileHitTower()
    {
        if (CurrentState != BossState.Attacking)
            return;

        hasHitTower = true;
        ActivateShield();
    }

    public void OnProjectileReflected()
    {
        if (CurrentState != BossState.Attacking)
            return;

        CurrentState = BossState.Stunned;
        stunTimer = stunDuration;

        CanMove = false;
        movementResumeTimer = movementResumeDelay;
    }

    private void ActivateShield()
    {
        if (IsDefeated)
            return;

        CurrentState = BossState.Shielded;
        Health.CanTakeDamage = false;

        shield?.Activate();
    }

    public void OnShieldDestroyed()
    {
        if (CurrentState != BossState.Shielded)
            return;

        Health.CanTakeDamage = true;
        CurrentState = BossState.Idle;

        if (hasHitTower)
            return;

        attackTimer = AttackDelay;
    }

    protected override void OnDefeated()
    {
        CurrentState = BossState.Defeated;
        shield?.Deactivate();
    }
}
