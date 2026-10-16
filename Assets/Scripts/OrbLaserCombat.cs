using UnityEngine;

public class OrbLaserCombat : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float attacksPerSecond = 2f;
    [SerializeField] private float aoeRadius = 1f;

    [Header("Firing Input Setup")]
    [SerializeField] private bool holdToFire = true;
    [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;

    [Header("Targeting Layer")]
    [SerializeField] private LayerMask enemyLayer;

    private OrbAimController _aimController;
    private float _tickTimer;

    public bool IsFiring { get; private set; }
    public float CurrentDamage { get; private set; }
    public float CurrentAttacksPerSecond { get; private set; }
    public float CurrentAoeRadius { get; private set; }

    private void Awake()
    {
        _aimController = GetComponent<OrbAimController>();
        
        CurrentDamage = baseDamage;
        CurrentAttacksPerSecond = attacksPerSecond;
        CurrentAoeRadius = aoeRadius;
    }

    private void Update()
    {
        HandleInput();

        if (!IsFiring)
        {
            _tickTimer = 0f;
            return;
        }

        _tickTimer += Time.deltaTime;
        float interval = 1f / CurrentAttacksPerSecond;

        if (_tickTimer >= interval)
        {
            _tickTimer -= interval;
            ProcessDamageTick();
        }
    }

    private void HandleInput()
    {
        if (holdToFire)
        {
            IsFiring = Input.GetKey(fireKey);
        }
        else
        {
            if (Input.GetKeyDown(fireKey))
            {
                IsFiring = !IsFiring;
            }
        }
    }

    public void SetFiringState(bool enable)
    {
        IsFiring = enable;
    }

    private void ProcessDamageTick()
    {
        Vector2 impactPoint = _aimController.TargetWorldPosition;
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(impactPoint, CurrentAoeRadius, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent<MockEnemyTarget>(out var enemy))
            {
                enemy.TakeDamage(CurrentDamage);
            }
        }
    }

    public void ApplyUpgrade()
    {
        CurrentAttacksPerSecond += 0.1f;
        CurrentDamage += 1f;
        CurrentAoeRadius += 2f;
    }

    private void OnDrawGizmosSelected()
    {
        if (_aimController != null && IsFiring)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_aimController.TargetWorldPosition, CurrentAoeRadius);
        }
    }
}