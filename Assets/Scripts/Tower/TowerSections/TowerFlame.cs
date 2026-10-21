
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerFlame : TowerSection
{
    [Header("Flame Settings")]
    [SerializeField] private ParticleSystem[] particleSystemRef;
    [SerializeField] private float playDuration = 2f;
    [SerializeField] private float cooldownTime = 3f;

    private Coroutine flameRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (particleSystemRef == null)
            particleSystemRef = GetComponents<ParticleSystem>();

        if (particleSystemRef != null)
        {   
            foreach(ParticleSystem particle in particleSystemRef)
                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    protected override void Start()
    {
        flameRoutine = StartCoroutine(PlayFlameRoutine());
    }

    protected override List<Enemy> FindTargets()
    {
        return new List<Enemy>();
    }

    protected override void Attack(List<Enemy> targets)
    {
    }

    private IEnumerator PlayFlameRoutine()
    {
        while (true)
        {
            if (particleSystemRef != null)
            {
                foreach(ParticleSystem particle in particleSystemRef)
                    particle.Play();
            }
                
            yield return new WaitForSeconds(playDuration);

            if (particleSystemRef != null)
            {   
                foreach(ParticleSystem particle in particleSystemRef)
                    particle.Stop(
                        true,
                        ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }

            yield return new WaitForSeconds(cooldownTime);
        }
    }

    private void OnDisable()
    {
        if (flameRoutine != null)
        {
            StopCoroutine(flameRoutine);
            flameRoutine = null;
        }

        if (particleSystemRef != null)
        {   
            foreach(ParticleSystem particle in particleSystemRef)
                particle.Stop(
                    true,
                    ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }

    public override void ApplyUpgrade(bool isSpecialUpgrade)
    {
        if (isSpecialUpgrade)
        {
            foreach (ParticleSystem particle in particleSystemRef)
            {
                if (particle == null)
                    continue;

                var main = particle.main;
                main.startLifetimeMultiplier += .2f;
            }
        }
        else
        {
            Damage += 1;
            playDuration += .1f;
            cooldownTime += .1f;
        }
    }
}
