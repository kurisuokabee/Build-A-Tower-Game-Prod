using UnityEngine;

public class AttackableHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;


    private float currentHealth;
    private float stunTimer;
    
    private PooledObject pooledObject;
    private AttackableReward reward;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;
    public bool IsStunned => stunTimer > 0f;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        reward = GetComponent<AttackableReward>();
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        stunTimer = 0f;

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
        if (!IsAlive || damage <= 0f)
            return;

        currentHealth = Mathf.Max(currentHealth - damage, 0f);

        if (currentHealth <= 0f)
            Die();
    }

    private void Die()
    {
        if (reward != null)
            reward.GiveReward();

        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        stunTimer = 0f;
    }

    public void InitializeHealth(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
    }
}