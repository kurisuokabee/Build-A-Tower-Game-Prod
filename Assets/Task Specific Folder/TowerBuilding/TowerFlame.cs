using System.Collections;
using UnityEngine;

public class TowerFlame : MonoBehaviour
{   

    public ParticleSystem particleSystemRef;
    public float playDuration = 2.0f;  
    public float cooldownTime = 3.0f;    

    void Start()
    {
        if (particleSystemRef == null)
        {
            particleSystemRef = GetComponent<ParticleSystem>();
        }

        StartCoroutine(PlayParticleRoutine());
    }

    IEnumerator PlayParticleRoutine()
    {
        while (true)
        {
            // Play the particle effect
            particleSystemRef.Play();

            // Wait for the active play duration
            yield return new WaitForSeconds(playDuration);

            // Stop the particle effect
            particleSystemRef.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            // Wait for the cooldown period
            yield return new WaitForSeconds(cooldownTime);
        }
    }
}
