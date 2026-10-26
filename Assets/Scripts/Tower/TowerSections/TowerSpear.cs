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

    private Vector3 leftRestPosition;
    private Vector3 rightRestPosition;

    private Vector3 leftHandlePosition;
    private Vector3 rightHandlePosition;

    private Vector3 leftHandleScale;
    private Vector3 rightHandleScale;

    private Coroutine attackRoutine;

    protected override void Awake()
    {
        base.Awake();

        if (leftSpear != null)
            leftRestPosition = leftSpear.localPosition;

        if (rightSpear != null)
            rightRestPosition = rightSpear.localPosition;

        if (leftHandleSprite != null)
        {
            leftHandlePosition = leftHandleSprite.localPosition;
            leftHandleScale = leftHandleSprite.localScale;
        }

        if (rightHandleSprite != null)
        {
            rightHandlePosition = rightHandleSprite.localPosition;
            rightHandleScale = rightHandleSprite.localScale;
        }

        UpdateHandleLengths();
    }

    protected override List<Enemy> FindTargets()
    {
        List<Enemy> targets = new List<Enemy>();

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distanceX = Mathf.Abs(
                enemy.transform.position.x - transform.position.x
            );

            float distanceY = Mathf.Abs(
                enemy.transform.position.y - transform.position.y
            );

            if (distanceX <= spearReach && distanceY <= 0.5f)
                targets.Add(enemy);
        }

        return targets;
    }

    protected override void Attack(List<Enemy> targets)
    {
        if (leftSpear == null || rightSpear == null)
            return;

        if (attackRoutine != null)
            return;

        attackRoutine = StartCoroutine(ThrustRoutine(targets));
    }

    private IEnumerator ThrustRoutine(List<Enemy> targets)
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

        foreach (Enemy enemy in targets)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distanceX = Mathf.Abs(
                enemy.transform.position.x - transform.position.x
            );

            float distanceY = Mathf.Abs(
                enemy.transform.position.y - transform.position.y
            );

            if (distanceX <= spearReach && distanceY <= 0.5f)
            {
                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();

                enemyHealth.TakeDamage(Damage);
            }
        }

        yield return MoveSpears(
            leftExtended,
            leftRestPosition,
            rightExtended,
            rightRestPosition,
            retractDuration
        );

        attackRoutine = null;
    }

    private IEnumerator MoveSpears(
        Vector3 leftFrom,
        Vector3 leftTo,
        Vector3 rightFrom,
        Vector3 rightTo,
        float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            leftSpear.localPosition = Vector3.Lerp(leftFrom, leftTo, t);
            rightSpear.localPosition = Vector3.Lerp(rightFrom, rightTo, t);

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
            -1f
        );

        UpdateHandleLength(
            rightSpear,
            rightHandleSprite,
            rightRestPosition,
            rightHandlePosition,
            rightHandleScale,
            1f
        );
    }

    private void UpdateHandleLength(
        Transform spear,
        Transform handle,
        Vector3 restPosition,
        Vector3 originalPosition,
        Vector3 originalScale,
        float direction)
    {
        if (spear == null || handle == null)
            return;

        SpriteRenderer renderer = handle.GetComponent<SpriteRenderer>();

        if (renderer == null || renderer.sprite == null)
            return;

        float originalWidth = renderer.sprite.bounds.size.x;

        if (originalWidth <= 0f)
            return;

        float extension = Mathf.Abs(
            spear.localPosition.x - restPosition.x
        );

        Vector3 scale = originalScale;
        scale.x = originalScale.x *
            (originalWidth * Mathf.Abs(originalScale.x) + extension) /
            (originalWidth * Mathf.Abs(originalScale.x));

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

        ResetHandle(leftHandleSprite, leftHandlePosition, leftHandleScale);
        ResetHandle(rightHandleSprite, rightHandlePosition, rightHandleScale);
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
        AttacksPerSecond =+ 1f;
    }
}