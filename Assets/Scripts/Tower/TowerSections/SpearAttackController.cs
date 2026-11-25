using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpearAttackController : MonoBehaviour
{
    [Header("Spear References")]
    [SerializeField] private Transform leftSpear;
    [SerializeField] private Transform rightSpear;
    [SerializeField] private Transform leftHandleSprite;
    [SerializeField] private Transform rightHandleSprite;
    [SerializeField] private Transform leftHead;
    [SerializeField] private Transform rightHead;

    [Header("Attack Settings")]
    [SerializeField] private float thrustDuration = 0.15f;
    [SerializeField] private float retractDuration = 0.15f;
    [SerializeField] private float hitRadius = 0.35f;

    private Vector3 leftRestPosition;
    private Vector3 rightRestPosition;

    private Vector3 leftHandlePosition;
    private Vector3 rightHandlePosition;

    private Vector3 leftHandleScale;
    private Vector3 rightHandleScale;

    private float leftHandleBaseWidth;
    private float rightHandleBaseWidth;
    private TowerSection towerSection;

    private readonly HashSet<AttackableHealth> hitTargets = new();

    private SpriteRenderer leftHandleRenderer;
    private SpriteRenderer rightHandleRenderer;

    private Coroutine attackRoutine;

    public bool IsAttacking => attackRoutine != null;

    private void Awake()
    {
        if (leftSpear != null)
            leftRestPosition = leftSpear.localPosition;

        if (rightSpear != null)
            rightRestPosition = rightSpear.localPosition;

        towerSection = GetComponent<TowerSection>();
        
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

    public void StartAttack(
        float reach,
        float damage,
        bool isRuthless,
        bool isRuthlessAttack,
        bool isStunning,
        float stunDuration)
    {
        if (IsAttacking)
            return;

        if (leftSpear == null || rightSpear == null)
        {
            Debug.LogError(
                "SpearAttackController is missing a spear reference.",
                this
            );
            return;
        }

        hitTargets.Clear();

        attackRoutine = StartCoroutine(
            ThrustRoutine(
                reach,
                damage,
                isRuthless && isRuthlessAttack,
                isStunning,
                stunDuration
            )
        );
    }

    private IEnumerator ThrustRoutine(
        float reach,
        float damage,
        bool doubleDamage,
        bool isStunning,
        float stunDuration)
    {
        Vector3 leftExtended =
            leftRestPosition + Vector3.left * reach;

        Vector3 rightExtended =
            rightRestPosition + Vector3.right * reach;

        yield return MoveSpears(
            leftRestPosition,
            leftExtended,
            rightRestPosition,
            rightExtended,
            thrustDuration,
            true,
            damage,
            doubleDamage,
            isStunning,
            stunDuration
        );

        yield return MoveSpears(
            leftExtended,
            leftRestPosition,
            rightExtended,
            rightRestPosition,
            retractDuration,
            false,
            damage,
            doubleDamage,
            isStunning,
            stunDuration
        );

        hitTargets.Clear();
        attackRoutine = null;
    }

    private IEnumerator MoveSpears(
        Vector3 leftFrom,
        Vector3 leftTo,
        Vector3 rightFrom,
        Vector3 rightTo,
        float duration,
        bool detectHits,
        float damage,
        bool doubleDamage,
        bool isStunning,
        float stunDuration)
    {
        if (duration <= 0f)
        {
            leftSpear.localPosition = leftTo;
            rightSpear.localPosition = rightTo;

            UpdateHandleLengths();

            if (detectHits)
            {
                DetectHits(
                    damage,
                    doubleDamage,
                    isStunning,
                    stunDuration
                );
            }

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

            if (detectHits)
            {
                DetectHits(
                    damage,
                    doubleDamage,
                    isStunning,
                    stunDuration
                );
            }

            yield return null;
        }

        leftSpear.localPosition = leftTo;
        rightSpear.localPosition = rightTo;

        UpdateHandleLengths();

        if (detectHits)
        {
            DetectHits(
                damage,
                doubleDamage,
                isStunning,
                stunDuration
            );
        }
    }

    private void DetectHits(
        float damage,
        bool doubleDamage,
        bool isStunning,
        float stunDuration)
    {
        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        Vector2 leftPosition = GetHeadPosition(leftHead, leftSpear);
        Vector2 rightPosition = GetHeadPosition(rightHead, rightSpear);

        float hitRadiusSquared = hitRadius * hitRadius;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive ||
                hitTargets.Contains(target))
            {
                continue;
            }

            Collider2D targetCollider =
                target.GetComponent<Collider2D>();

            Vector2 leftClosestPoint = targetCollider != null
                ? targetCollider.ClosestPoint(leftPosition)
                : (Vector2)target.transform.position;

            Vector2 rightClosestPoint = targetCollider != null
                ? targetCollider.ClosestPoint(rightPosition)
                : (Vector2)target.transform.position;

            bool hitLeft =
                (leftClosestPoint - leftPosition).sqrMagnitude
                <= hitRadiusSquared;

            bool hitRight =
                (rightClosestPoint - rightPosition).sqrMagnitude
                <= hitRadiusSquared;

            if (!hitLeft && !hitRight)
                continue;

            hitTargets.Add(target);

            float finalDamage = doubleDamage
                ? damage * 2f
                : damage;

        
            towerSection?.ApplyWandImpact(target);

            if (target.IsAlive)
                target.TakeDamage(finalDamage);

            if (target.IsAlive && isStunning)
                target.ApplyStun(stunDuration);
        }
    }

    private Vector2 GetHeadPosition(
        Transform head,
        Transform spear)
    {
        return head != null
            ? head.position
            : spear.position;
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

        scale.x = originalScale.x *
            (baseWidth + extension) / baseWidth;

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

        hitTargets.Clear();
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
}
