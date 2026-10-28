
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerSpear : TowerSection
{
    [Header("Spear References")]
    [SerializeField] private Transform leftSpear;
    [SerializeField] private Transform rightSpear;
    [SerializeField] private Transform leftHandleSprite;
    [SerializeField] private Transform rightHandleSprite;
    [SerializeField] private Transform leftHead;
    [SerializeField] private Transform rightHead;

    [Header("Attack Settings")]
    [SerializeField] private float spearReach = 2f;
    [SerializeField] private float thrustDuration = 0.15f;
    [SerializeField] private float retractDuration = 0.15f;
    [SerializeField] private float verticalHitRange = 0.5f;

    private Vector3 leftRestPosition;
    private Vector3 rightRestPosition;

    private Vector3 leftHandlePosition;
    private Vector3 rightHandlePosition;

    private Vector3 leftHandleScale;
    private Vector3 rightHandleScale;

    private SpriteRenderer leftHandleRenderer;
    private SpriteRenderer rightHandleRenderer;

    private float leftHandleBaseWidth;
    private float rightHandleBaseWidth;

    private readonly List<AttackableHealth> targetBuffer = new();

    private Coroutine attackRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (leftSpear != null)
            leftRestPosition = leftSpear.localPosition;

        if (rightSpear != null)
            rightRestPosition = rightSpear.localPosition;

        CacheHandle(
            leftHandleSprite,
            out leftHandlePosition,
            out leftHandleScale,
            out leftHandleRenderer,
            out leftHandleBaseWidth
        );

        CacheHandle(
            rightHandleSprite,
            out rightHandlePosition,
            out rightHandleScale,
            out rightHandleRenderer,
            out rightHandleBaseWidth
        );

        UpdateHandleLengths();
    }

    private void CacheHandle(
        Transform handle,
        out Vector3 originalPosition,
        out Vector3 originalScale,
        out SpriteRenderer renderer,
        out float baseWidth)
    {
        originalPosition = handle != null
            ? handle.localPosition
            : Vector3.zero;

        originalScale = handle != null
            ? handle.localScale
            : Vector3.one;

        renderer = handle != null
            ? handle.GetComponent<SpriteRenderer>()
            : null;

        baseWidth = 0f;

        if (renderer != null && renderer.sprite != null)
        {
            baseWidth =
                renderer.sprite.bounds.size.x *
                Mathf.Abs(originalScale.x);
        }
    }

    protected override IReadOnlyList<AttackableHealth> FindTargets()
    {
        targetBuffer.Clear();

        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        float originX = transform.position.x;
        float originY = transform.position.y;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive)
            {
                continue;
            }

            Vector3 position = target.transform.position;

            if (Mathf.Abs(position.x - originX) <= spearReach &&
                Mathf.Abs(position.y - originY) <= verticalHitRange)
            {
                targetBuffer.Add(target);
            }
        }

        return targetBuffer;
    }

    protected override void Attack(
        IReadOnlyList<AttackableHealth> targets)
    {
        if (leftSpear == null ||
            rightSpear == null ||
            attackRoutine != null)
        {
            return;
        }

        attackRoutine = StartCoroutine(ThrustRoutine());
    }

    private IEnumerator ThrustRoutine()
    {
        Vector3 leftExtended =
            leftRestPosition + Vector3.left * spearReach;

        Vector3 rightExtended =
            rightRestPosition + Vector3.right * spearReach;

        yield return MoveSpears(
            leftRestPosition,
            leftExtended,
            rightRestPosition,
            rightExtended,
            thrustDuration
        );

        DamageTargetsInRange();

        yield return MoveSpears(
            leftExtended,
            leftRestPosition,
            rightExtended,
            rightRestPosition,
            retractDuration
        );

        attackRoutine = null;
    }

    private void DamageTargetsInRange()
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        float originX = transform.position.x;
        float originY = transform.position.y;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive)
            {
                continue;
            }

            Vector3 position = target.transform.position;

            if (Mathf.Abs(position.x - originX) <= spearReach &&
                Mathf.Abs(position.y - originY) <= verticalHitRange)
            {
                target.TakeDamage(EffectiveDamage);
            }
        }
    }

    private IEnumerator MoveSpears(
        Vector3 leftFrom,
        Vector3 leftTo,
        Vector3 rightFrom,
        Vector3 rightTo,
        float duration)
    {
        if (duration <= 0f)
        {
            leftSpear.localPosition = leftTo;
            rightSpear.localPosition = rightTo;
            UpdateHandleLengths();
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            leftSpear.localPosition =
                Vector3.Lerp(leftFrom, leftTo, t);

            rightSpear.localPosition =
                Vector3.Lerp(rightFrom, rightTo, t);

            UpdateHandleLengths();

            yield return null;
        }

        leftSpear.localPosition = leftTo;
        rightSpear.localPosition = rightTo;

        UpdateHandleLengths();
    }

    private void UpdateHandleLengths()
    {
        UpdateHandleLength(
            leftSpear,
            leftHandleSprite,
            leftRestPosition,
            leftHandlePosition,
            leftHandleScale,
            leftHandleBaseWidth,
            -1f
        );

        UpdateHandleLength(
            rightSpear,
            rightHandleSprite,
            rightRestPosition,
            rightHandlePosition,
            rightHandleScale,
            rightHandleBaseWidth,
            1f
        );
    }

    private void UpdateHandleLength(
        Transform spear,
        Transform handle,
        Vector3 restPosition,
        Vector3 originalPosition,
        Vector3 originalScale,
        float baseWidth,
        float direction)
    {
        if (spear == null || handle == null || baseWidth <= 0f)
            return;

        float extension = Mathf.Abs(
            spear.localPosition.x - restPosition.x
        );

        Vector3 scale = originalScale;
        scale.x = originalScale.x * (baseWidth + extension) / baseWidth;
        handle.localScale = scale;

        Vector3 position = originalPosition;
        position.x += -direction * extension * 0.5f;
        handle.localPosition = position;
    }

    private void OnDisable()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        if (leftSpear != null)
            leftSpear.localPosition = leftRestPosition;

        if (rightSpear != null)
            rightSpear.localPosition = rightRestPosition;

        ResetHandle(
            leftHandleSprite,
            leftHandlePosition,
            leftHandleScale
        );

        ResetHandle(
            rightHandleSprite,
            rightHandlePosition,
            rightHandleScale
        );
    }

    private void ResetHandle(
        Transform handle,
        Vector3 originalPosition,
        Vector3 originalScale)
    {
        if (handle == null)
            return;

        handle.localPosition = originalPosition;
        handle.localScale = originalScale;
    }

    public override void ApplyUpgrade()
    {
        Damage += 1f;
        AttacksPerSecond += 0.1f;
    }
}
