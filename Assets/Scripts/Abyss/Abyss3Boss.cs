
using UnityEngine;

public class Abyss3Boss : BossController
{
    [Header("Invulnerability Progression")]
    [SerializeField, Min(1)] private int startingMinionKills = 15;
    [SerializeField, Min(0)] private int additionalKillsPerCycle = 5;

    [Header("Vulnerability Duration")]
    [SerializeField, Min(1f)] private float startingVulnerabilityDuration = 15f;
    [SerializeField, Min(0f)] private float cooldownReductionPerCycle = 3f;
    [SerializeField, Min(1f)] private float minimumVulnerabilityDuration = 5f;

    private int currentCycle = 1;
    private int minionsKilled;
    private float vulnerabilityTimer;
    private bool isVulnerable;

    public int CurrentCycle => currentCycle;
    public int MinionsKilled => minionsKilled;
    public bool IsVulnerable => isVulnerable;

    public int RequiredMinionKills =>
        startingMinionKills +
        ((currentCycle - 1) * additionalKillsPerCycle);

    public float VulnerabilityDuration =>
        Mathf.Max(
            minimumVulnerabilityDuration,
            startingVulnerabilityDuration -
            ((currentCycle - 1) * cooldownReductionPerCycle)
        );

    public float VulnerabilityTimer => vulnerabilityTimer;

    protected override void OnEnable()
    {
        base.OnEnable();

        currentCycle = 1;
        minionsKilled = 0;
        vulnerabilityTimer = 0f;
        isVulnerable = false;

        if (Health != null)
            Health.CanTakeDamage = false;
    }

    public override void Initialize(BossData data)
    {
        base.Initialize(data);

        if (Health == null)
            return;

        currentCycle = 1;
        minionsKilled = 0;
        vulnerabilityTimer = 0f;
        isVulnerable = false;

        Health.CanTakeDamage = false;
    }

    private void Update()
    {
        if (IsDefeated || !isVulnerable)
            return;

        vulnerabilityTimer -= Time.deltaTime;

        if (vulnerabilityTimer <= 0f)
            BeginNextCycle();
    }

    public void RegisterMinionKill()
    {
        if (IsDefeated || isVulnerable)
            return;

        minionsKilled++;

        Debug.Log(
            $"Abyss 3: Minions killed {minionsKilled}/{RequiredMinionKills}"
        );

        if (minionsKilled >= RequiredMinionKills)
            BecomeVulnerable();
    }

    private void BecomeVulnerable()
    {
        isVulnerable = true;
        vulnerabilityTimer = VulnerabilityDuration;

        Health.CanTakeDamage = true;

        Debug.Log(
            $"Abyss 3 boss is vulnerable for {VulnerabilityDuration} seconds!"
        );
    }

    private void BeginNextCycle()
    {
        currentCycle++;
        minionsKilled = 0;
        isVulnerable = false;
        vulnerabilityTimer = 0f;

        Health.CanTakeDamage = false;

        Debug.Log(
            $"Abyss 3: Cycle {currentCycle} started. " +
            $"Required kills: {RequiredMinionKills}"
        );
    }
}
