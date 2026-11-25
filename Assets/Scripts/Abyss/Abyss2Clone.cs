using UnityEngine;

public class Abyss2Clone : MonoBehaviour
{
    private Abyss2Boss bossController;
    private AttackableHealth ownHealth;
    private bool isOriginal;

    public bool IsMainBoss { get; private set; }

    [Header("Main Boss Bobbing")]
    [SerializeField] private float bobbingAmount = 0.05f;
    [SerializeField] private float bobbingSpeed = 2f;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (!IsMainBoss)
            return;

        float offset = Mathf.Sin(Time.time * bobbingSpeed) * bobbingAmount;

        transform.localScale = new Vector3(
            originalScale.x * (1f + offset),
            originalScale.y * (1f - offset),
            originalScale.z
        );
    }

    public void Initialize(Abyss2Boss boss, bool isOriginal)
    {
        bossController = boss;
        this.isOriginal = isOriginal;

        ownHealth = GetComponent<AttackableHealth>();

        originalScale = transform.localScale;

        SetMainBoss(false);
    }

    public void SetMainBoss(bool isMainBoss)
    {
        IsMainBoss = isMainBoss;

        if (ownHealth == null)
            ownHealth = GetComponent<AttackableHealth>();

        if (!isOriginal && ownHealth != null)
            ownHealth.CanTakeDamage = isMainBoss;

        if (!isMainBoss)
            transform.localScale = originalScale;
    }

    public bool TryForwardDamage(float damage)
    {
        if (!IsMainBoss || bossController == null)
            return true;

        if (isOriginal)
            return false;

        bossController.SharedHealth.TakeSharedDamage(damage);
        return true;
    }
}