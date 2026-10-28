
using UnityEngine;

public class AttackableHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField, Min(1f)] private float maxHealth = 100f;

    private float currentHealth;
    private PooledObject pooledObject;
    private AttackableReward reward;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsAlive => currentHealth > 0f;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
        reward = GetComponent<AttackableReward>();
        currentHealth = maxHealth;
    }

    private void OnEnable()
    {
        currentHealth = maxHealth;
        AttackableTargetRegistry.Register(this);
    }

    private void OnDisable()
    {
        AttackableTargetRegistry.Unregister(this);
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
    }

    public void InitializeHealth(float health)
    {
        maxHealth = Mathf.Max(1f, health);
        currentHealth = maxHealth;
    }

    

    
}
