
using System;
using UnityEngine;

[RequireComponent(typeof(AttackableHealth))]
public class BossController : MonoBehaviour
{
    [Header("Boss References")]
    [SerializeField] private AttackableHealth health;

    public AttackableHealth Health => health;
    public bool IsDefeated { get; private set; }

    public event Action OnBossDefeated;

    protected virtual void Awake()
    {
        if (health == null)
            health = GetComponent<AttackableHealth>();
    }

    protected virtual void OnEnable()
    {
        IsDefeated = false;

        if (health != null)
            health.OnDied += HandleDefeated;
    }

    protected virtual void OnDisable()
    {
        if (health != null)
            health.OnDied -= HandleDefeated;
    }

    public virtual void Initialize(BossData data)
    {
        if (data == null || health == null)
            return;

        IsDefeated = false;
        health.InitializeHealth(data.MaxHealth);
        health.CanTakeDamage = true;
    }

    private void HandleDefeated()
    {
        if (IsDefeated)
            return;

        IsDefeated = true;
        OnBossDefeated?.Invoke();

        OnDefeated();
    }

    protected virtual void OnDefeated()
    {
    }
}
