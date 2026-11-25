
using UnityEngine;

public class SakeroFallingAnimation : MonoBehaviour
{
    [Header("Scale")]
    [SerializeField] private float minScale = 0.75f;
    [SerializeField] private float maxScale = 1.25f;
    [SerializeField] private float stretchAmount = 0.2f;

    [Header("Vertical Movement")]
    [SerializeField] private float floatAmount = 0.4f;
    [SerializeField] private float floatSpeed = 3f;

    [Header("Rotation")]
    [SerializeField] private float maxRotation = 35f;
    [SerializeField] private float rotationSpeed = 3f;

    [Header("Randomization")]
    [SerializeField] private float changeInterval = 0.5f;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    private float currentScale = 1f;
    private float targetScale;
    private float currentStretch;
    private float targetStretch;
    private float targetRotation;
    private float targetFloat;

    private float timer;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;

        RandomizeTargets();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= changeInterval)
        {
            timer = 0f;
            RandomizeTargets();
        }

        currentScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * floatSpeed);
        currentStretch = Mathf.Lerp(currentStretch, targetStretch, Time.deltaTime * floatSpeed);

        float horizontalScale = currentScale * (1f - currentStretch);
        float verticalScale = currentScale * (1f + currentStretch);

        transform.localScale = new Vector3(
            initialScale.x * horizontalScale,
            initialScale.y * verticalScale,
            initialScale.z
        );

        float rotation = Mathf.LerpAngle(
            transform.localEulerAngles.z,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );

        transform.localRotation = Quaternion.Euler(0f, 0f, rotation);

        Vector3 targetPosition = initialPosition + Vector3.up * targetFloat;

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * floatSpeed
        );
    }

    private void RandomizeTargets()
    {
        targetScale = Random.Range(minScale, maxScale);
        targetStretch = Random.Range(-stretchAmount, stretchAmount);
        targetFloat = Random.Range(-floatAmount, floatAmount);
        targetRotation = Random.Range(-maxRotation, maxRotation);
    }
}
