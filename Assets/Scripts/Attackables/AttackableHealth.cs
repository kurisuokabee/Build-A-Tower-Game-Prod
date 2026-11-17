
using System;
using UnityEngine;

public class AttackableHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private bool giveRewards = true;

    private float currentHealth;
    private float stunTimer;

    private PooledObject pooledObject;
    private AttackableReward reward;
    private Abyss2Clone abyss2Clone;
    private Abyss3Minion abyss3Minion;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsStunned => stunTimer > 0f;

    public bool CanTakeDamage { get; set; } = true;

    public event Action OnDied;

    [SerializeField] private bool returnToPoolOnDeath = true;

    public bool ReturnToPoolOnDeath
    {
        get => returnToPoolOnDeath;
        set => returnToPoolOnDeath = value;
    }
    
    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        reward = GetComponent<AttackableReward>();
        abyss2Clone = GetComponent<Abyss2Clone>();
        abyss3Minion = GetComponent<Abyss3Minion>();

        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        stunTimer = 0f;
        CanTakeDamage = true;

        AttackableTargetRegistry.Register(this);
    }

    private void OnDisable()
    {
        AttackableTargetRegistry.Unregister(this);
        stunTimer = 0f;
    }

    private void Update()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer < 0f)
                stunTimer = 0f;
        }
    }

    public void ApplyStun(float duration)
    {
        if (duration <= 0f || !IsAlive)
            return;

        stunTimer = Mathf.Max(stunTimer, duration);
    }

    
    public void TakeDamage(float damage)
    {
        if (!IsAlive || !CanTakeDamage || damage <= 0f)
            return;

        if (abyss2Clone != null &&
            abyss2Clone.TryForwardDamage(damage))
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
            Die();
    }


    public void TakeSharedDamage(float damage)
    {
        if (!IsAlive || !CanTakeDamage || damage <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
            Die();
    }


    private void Die()
    {
        if (giveRewards && reward != null)
            reward.GiveReward();

        Abyss3Minion minion = GetComponent<Abyss3Minion>();

        if (minion != null)
            minion.RegisterKill();

        OnDied?.Invoke();

        if (!returnToPoolOnDeath)
            return;

        if (pooledObject != null)
            pooledObject.ReturnToPool();
        else if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        else
            gameObject.SetActive(false);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        stunTimer = 0f;
        CanTakeDamage = true;
    }

    public void InitializeHealth(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
    }
}
