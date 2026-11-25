
using System.Collections;
using UnityEngine;

public class SakeroLandingIntro : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private ParticleSystem landingEffect;

    [Header("Landing")]
    [SerializeField] private float fallDuration = 1f;
    [SerializeField] private float startHeightOffset = 2f;

    [Header("Impact")]
    [SerializeField] private float squashAmount = 0.7f;
    [SerializeField] private float stretchAmount = 1.25f;
    [SerializeField] private float recoveryDuration = 0.2f;

    
    private Vector3 initialPosition;
    private Vector3 initialScale;

    private void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 topPosition = mainCamera.ViewportToWorldPoint(
            new Vector3(0.5f, 1f, mainCamera.WorldToViewportPoint(initialPosition).z)
        );

        topPosition.y += startHeightOffset;
        topPosition.x = initialPosition.x;
        topPosition.z = initialPosition.z;

        transform.position = topPosition;

        StartCoroutine(PlayLanding());
    }

    private IEnumerator PlayLanding()
    {
        Vector3 startPosition = transform.position;
        float timer = 0f;

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fallDuration);
            float eased = t * t;

            transform.position = Vector3.Lerp(
                startPosition,
                initialPosition,
                eased
            );

            yield return null;
        }

        transform.position = initialPosition;

        if (landingEffect != null)
            landingEffect.Play();

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake();

        Vector3 squashedScale = new Vector3(
            initialScale.x * stretchAmount,
            initialScale.y * squashAmount,
            initialScale.z
        );

        transform.localScale = squashedScale;

        float recoveryTimer = 0f;

        while (recoveryTimer < recoveryDuration)
        {
            recoveryTimer += Time.deltaTime;

            transform.localScale = Vector3.Lerp(
                squashedScale,
                initialScale,
                recoveryTimer / recoveryDuration
            );

            yield return null;
        }

        transform.localScale = initialScale;
    }
}
