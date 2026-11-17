
using UnityEngine;

[RequireComponent(typeof(AttackableHealth))]
public class BossProjectile : MonoBehaviour
{
    public enum ProjectileState
    {
        MovingToTower,
        Reflected,
        Finished
    }

    [Header("References")]
    [SerializeField] private AttackableHealth health;

    [Header("Movement")]
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField, Min(0.01f)] private float hitDistance = 0.1f;

    private Transform towerTarget;
    private Abyss1Boss bossController;
    private Abyss1AttackController attackController;

    public ProjectileState CurrentState { get; private set; }

    private void Awake()
    {
        if (health == null)
            health = GetComponent<AttackableHealth>();

        health.OnDied += HandleDestroyed;
        health.ReturnToPoolOnDeath = false;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDied -= HandleDestroyed;
    }

    private void OnEnable()
    {
        CurrentState = ProjectileState.MovingToTower;
    }

    public void Initialize(
        Transform tower,
        float speed,
        Abyss1Boss boss,
        Abyss1AttackController controller)
    {
        towerTarget = tower;
        movementSpeed = speed;
        bossController = boss;
        attackController = controller;

        CurrentState = ProjectileState.MovingToTower;

        health.ReturnToPoolOnDeath = false;
        health.ResetHealth();
        health.CanTakeDamage = true;
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case ProjectileState.MovingToTower:
                MoveTowardsTower();
                break;

            case ProjectileState.Reflected:
                MoveTowardsBoss();
                break;
        }
    }

    private void MoveTowardsTower()
    {
        if (towerTarget == null)
        {
            FinishProjectile();
            return;
        }

        MoveTowards(towerTarget.position, 0);

        if (Vector3.Distance(transform.position, towerTarget.position) <= hitDistance)
            HitTower();
    }

    private void MoveTowardsBoss()
    {
        if (bossController == null)
        {
            FinishProjectile();
            return;
        }

        MoveTowards(bossController.transform.position, 5);

        if (Vector3.Distance(transform.position, bossController.transform.position) <= hitDistance)
            HitBoss();
    }

    private void MoveTowards(Vector3 targetPosition, int addedMovement)
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            (movementSpeed + addedMovement) * Time.deltaTime
        );
    }

    private void HandleDestroyed()
    {
        if (CurrentState != ProjectileState.MovingToTower)
            return;

        CurrentState = ProjectileState.Reflected;
        health.CanTakeDamage = false;
    }

    private void HitTower()
    {
        if (CurrentState != ProjectileState.MovingToTower)
            return;

        PlayerEconomy economy = PlayerEconomy.Instance;

        if (economy != null)
            economy.SpendBlocks(economy.Blocks);

        bossController?.OnProjectileHitTower();

        FinishProjectile();
    }

    
    private void HitBoss()
    {
        if (CurrentState != ProjectileState.Reflected)
            return;

        Debug.Log("Reflected projectile hit the boss!");

        if (bossController != null)
        {
            Debug.Log($"Boss state before hit: {bossController.CurrentState}");
            bossController.OnProjectileReflected();
            Debug.Log($"Boss state after hit: {bossController.CurrentState}");
        }
        else
        {
            Debug.LogWarning("BossController reference is missing!");
        }

        FinishProjectile();
    }


    private void FinishProjectile()
    {
        if (CurrentState == ProjectileState.Finished)
            return;

        CurrentState = ProjectileState.Finished;

        attackController?.OnProjectileFinished(this);

        PooledObject pooledObject = GetComponent<PooledObject>();

        if (pooledObject != null)
            pooledObject.ReturnToPool();
        else if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        else
            gameObject.SetActive(false);
    }
}
