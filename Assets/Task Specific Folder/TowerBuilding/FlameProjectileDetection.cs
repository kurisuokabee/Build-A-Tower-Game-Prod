using UnityEngine;
using UnityEngine.SceneManagement;

public class FlameProjectileDetection : MonoBehaviour
{
    [SerializeField] ParticleSystem _particleSystem;
    
    ParticleSystem.Particle[] particles;

    Enemy[] enemies;

    void Start()
    {
        particles = new ParticleSystem.Particle[ _particleSystem.main.maxParticles ];

        enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
    }

    void Update()
    {
        int count = _particleSystem.GetParticles(particles);

        for (int i = 0; i < count; i++)
        {   
            foreach (Enemy enemy in enemies)
            {
                float distanceSqr = (particles[i].position - enemy.transform.position).sqrMagnitude;

                if (distanceSqr <= .5f * .5f)
                {
                    Debug.Log("Particle detected enemy!");
                    Enemy detectedEnemy = enemy.GetComponent<Enemy>();

                    //detectedEnemy.Die();
                    return;  
                }
            }
            
        }   
    }   
}