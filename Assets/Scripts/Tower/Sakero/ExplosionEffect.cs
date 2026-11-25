
using System.Collections;
using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem explosionParticles;
    [SerializeField] private ExplosionRing explosionRing;

    private Coroutine returnCoroutine;
    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Play(float explosionRadius)
    {
        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);

        if (explosionParticles != null)
        {
            explosionParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            explosionParticles.Play();
        }

        if (explosionRing != null)
            explosionRing.Play(explosionRadius);

        float particleDuration = 0f;

        if (explosionParticles != null)
        {
            var main = explosionParticles.main;

            particleDuration =
                main.duration + main.startLifetime.constantMax;
        }

        float ringDuration = explosionRing != null
            ? explosionRing.Duration
            : 0f;

        returnCoroutine = StartCoroutine(
            ReturnAfterDelay(Mathf.Max(particleDuration, ringDuration))
        );
    }

    private IEnumerator ReturnAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        returnCoroutine = null;

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

    private void OnDisable()
    {
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        if (explosionParticles != null)
        {
            explosionParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}
