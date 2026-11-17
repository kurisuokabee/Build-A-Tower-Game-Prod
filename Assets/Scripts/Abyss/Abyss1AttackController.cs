
using UnityEngine;

public class Abyss1AttackController : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 2f;

    [Header("References")]
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private Abyss1Boss bossController;

    private GameObject activeProjectile;

    public bool HasActiveProjectile => activeProjectile != null;

    private void Awake()
    {
        if (bossController == null)
            bossController = GetComponent<Abyss1Boss>();
    }

    private void OnDisable()
    {
        ReturnActiveProjectile();
    }

    
    public bool LaunchProjectile()
    {
        if (HasActiveProjectile)
        {
            Debug.LogWarning("AttackController: Another projectile is already active.");
            return false;
        }

        TowerBuilding tower = TowerBuilding.Instance;

        Vector3 spawnPosition = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position;

        activeProjectile = ObjectPoolManager.Instance.GetObject(
            projectilePrefab,
            spawnPosition,
            Quaternion.identity
        );

        if (activeProjectile == null)
        {
            Debug.LogError("AttackController: ObjectPoolManager failed to spawn projectile!");
            return false;
        }

        BossProjectile projectile = activeProjectile.GetComponent<BossProjectile>();

        if (projectile == null)
        {
            Debug.LogError("AttackController: Projectile prefab has no BossProjectile component!");
            ReturnActiveProjectile();
            return false;
        }

        projectile.Initialize(
            tower.StartingBase.transform,
            projectileSpeed,
            bossController,
            this
        );

        return true;
    }


    public void OnProjectileFinished(BossProjectile projectile)
    {
        if (projectile == null || activeProjectile != projectile.gameObject)
            return;

        activeProjectile = null;
    }

    private void ReturnActiveProjectile()
    {
        if (activeProjectile == null)
            return;

        GameObject projectile = activeProjectile;
        activeProjectile = null;

        PooledObject pooledObject = projectile.GetComponent<PooledObject>();

        if (pooledObject != null)
            pooledObject.ReturnToPool();
        else if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(projectile);
        else
            projectile.SetActive(false);
    }
}
