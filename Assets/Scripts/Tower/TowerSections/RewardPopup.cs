using UnityEngine;
using TMPro;

public class RewardPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float lifetime = 1f;

    private float timer;
    private Color originalColor;
    private Vector3 initialScale;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void Show(string message, Color color)
    {
        timer = 0f;

        originalColor = color;
        originalColor.a = 1f;

        rewardText.text = message;
        rewardText.color = originalColor;

        transform.localScale = initialScale;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        transform.position += Vector3.up * (floatSpeed * Time.deltaTime);

        Color color = originalColor;
        color.a = Mathf.Lerp(1f, 0f, timer / lifetime);
        rewardText.color = color;

        if (timer >= lifetime)
            ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}   