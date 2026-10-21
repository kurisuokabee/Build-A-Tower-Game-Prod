using System.Collections.Generic;
using UnityEngine;

public class FlameProjectileDetection : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem[] particleSystems = new ParticleSystem[2];

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private float hitRadius = 0.5f;

    private ParticleSystem.Particle[][] particleBuffers;

    TowerFlame towerFlame;

    // Tracks when each enemy can take damage again.
    private readonly Dictionary<EnemyHealth, float> damageTimers = new();

    private void Awake()
    {
        particleBuffers = new ParticleSystem.Particle[particleSystems.Length][];

        for (int i = 0; i < particleSystems.Length; i++)
        {
            if (particleSystems[i] == null)
                continue;

            particleBuffers[i] = new ParticleSystem.Particle[
                particleSystems[i].main.maxParticles
            ];
        }

        towerFlame = GetComponent<TowerFlame>();
    }

    private void Update()
    {
        DetectParticleHits();
    }

    private void DetectParticleHits()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        HashSet<EnemyHealth> hitThisFrame = new();

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem system = particleSystems[i];

            if (system == null || particleBuffers[i] == null)
                continue;

            int count = system.GetParticles(particleBuffers[i]);

            for (int p = 0; p < count; p++)
            {
                Vector3 particlePosition = particleBuffers[i][p].position;

                foreach (Enemy enemy in enemies)
                {
                    if (enemy == null || !enemy.gameObject.activeInHierarchy)
                        continue;

                    EnemyHealth health = enemy.GetComponent<EnemyHealth>();

                    if (health == null || hitThisFrame.Contains(health))
                        continue;

                    Vector3 offset = enemy.transform.position - particlePosition;

                    if (offset.sqrMagnitude > hitRadius * hitRadius)
                        continue;

                    hitThisFrame.Add(health);

                    if (!damageTimers.TryGetValue(health, out float nextDamageTime)
                        || Time.time >= nextDamageTime)
                    {
                        health.TakeDamage(towerFlame.Damage);
                        damageTimers[health] = Time.time + damageCooldown;
                    }
                }
            }
        }

        List<EnemyHealth> toRemove = new List<EnemyHealth>();

        foreach (var entry in damageTimers)
        {
            if (entry.Key == null || !entry.Key.gameObject.activeInHierarchy)
                toRemove.Add(entry.Key);
        }

        foreach (EnemyHealth enemy in toRemove)
        {
            damageTimers.Remove(enemy);
        }
    }

    private void OnDisable()
    {
        damageTimers.Clear();
    }
}
