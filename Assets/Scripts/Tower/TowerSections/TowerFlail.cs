
using System.Collections.Generic;
using UnityEngine;

public class TowerFlail : TowerSection
{
    [Header("Flail References")]
    [SerializeField] private Transform flailPivot;
    [SerializeField] private Transform flailBall;

    [Header("Flail Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float hitRadius = 0.5f;

    private float currentAngle;
    private float previousAngle;

    private readonly HashSet<AttackableHealth> hitTargets = new();
    private readonly List<AttackableHealth> targetsToRemove = new();

    protected override void Awake()
    {
        base.Awake();

        if (flailPivot == null)
            Debug.LogWarning("TowerFlail is missing its FlailPivot.", this);

        if (flailBall == null)
            Debug.LogWarning("TowerFlail is missing its FlailBall.", this);
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        return AttackableTargetRegistry.Targets;
    }

    
    protected override void Update()
    {
        if (flailPivot == null || flailBall == null)
            return;

        previousAngle = currentAngle;

        float rotationThisFrame = rotationSpeed * Time.deltaTime;
        currentAngle = (currentAngle + rotationThisFrame) % 360f;

        flailPivot.Rotate(0f, 0f, rotationThisFrame);

        if (currentAngle < previousAngle)
            hitTargets.Clear();

        DamageTargetsAlongPath(previousAngle, rotationThisFrame);
    }

    
    private void DamageTargetsAlongPath(
        float startAngle,
        float rotationThisFrame)
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        Vector2 center = flailPivot.position;
        float radiusSquared = hitRadius * hitRadius;

        float flailLength = Vector2.Distance(
            center,
            flailBall.position
        );

        // Sample the arc frequently enough to reduce missed hits.
        float arcLength = Mathf.Abs(rotationThisFrame * Mathf.Deg2Rad)
            * flailLength;

        int steps = Mathf.Max(
            1,
            Mathf.CeilToInt(arcLength / Mathf.Max(hitRadius, 0.05f))
        );

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive ||
                hitTargets.Contains(target))
            {
                continue;
            }

            Vector2 targetPosition = target.transform.position;
            bool hit = false;

            for (int step = 0; step <= steps; step++)
            {
                float t = step / (float)steps;
                float angle = startAngle + rotationThisFrame * t;

                float radians = angle * Mathf.Deg2Rad;

                Vector2 samplePosition = center + new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                ) * flailLength;

                if ((targetPosition - samplePosition).sqrMagnitude
                    <= radiusSquared)
                {
                    hit = true;
                    break;
                }
            }

            if (hit)
            {
                target.TakeDamage(EffectiveDamage);
                hitTargets.Add(target);
            }
        }
    }


    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
    }

    public override void ApplyUpgrade()
    {
        Damage += 1f;
    }

    private void OnDisable()
    {
        hitTargets.Clear();
        currentAngle = 0f;
        previousAngle = 0f;

        if (flailPivot != null)
            flailPivot.localRotation = Quaternion.identity;
    }
}
