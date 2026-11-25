
using System.Collections.Generic;
using UnityEngine;

public class FlailWeaponHitbox : MonoBehaviour
{
    private readonly Dictionary<AttackableHealth, HashSet<Collider2D>>
        touchingTargets = new();

    private TowerSection towerSection;

    private float damage;
    private bool isRoulette;
    private float minimumDamageMultiplier;
    private float maximumDamageMultiplier;

    private void Awake()
    {
        towerSection = GetComponentInParent<TowerSection>();
    }

    public void Configure(
        float damage,
        bool isRoulette,
        float minimumDamageMultiplier,
        float maximumDamageMultiplier)
    {
        this.damage = damage;
        this.isRoulette = isRoulette;
        this.minimumDamageMultiplier = minimumDamageMultiplier;
        this.maximumDamageMultiplier = maximumDamageMultiplier;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AttackableHealth target =
            other.GetComponentInParent<AttackableHealth>();

        if (target == null || !target.IsAlive)
            return;

        if (!touchingTargets.TryGetValue(
            target,
            out HashSet<Collider2D> colliders))
        {
            colliders = new HashSet<Collider2D>();
            touchingTargets.Add(target, colliders);
        }

        colliders.Add(other);

        if (colliders.Count > 1)
            return;

        float finalDamage = damage;

        if (isRoulette)
        {
            float multiplier = Random.Range(
                minimumDamageMultiplier,
                maximumDamageMultiplier
            );

            finalDamage *= multiplier;

            Debug.Log(
                $"[Roulette Flail] {multiplier:F2}x | Damage: {finalDamage:F2}",
                this
            );
        }

        // Apply Sakero's active wand effect first.
        towerSection?.ApplyWandImpact(target);

        // Apply the Flail's normal damage afterward.
        if (target.IsAlive)
            target.TakeDamage(finalDamage);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        AttackableHealth target =
            other.GetComponentInParent<AttackableHealth>();

        if (target == null)
            return;

        if (!touchingTargets.TryGetValue(
            target,
            out HashSet<Collider2D> colliders))
            return;

        colliders.Remove(other);

        if (colliders.Count == 0)
            touchingTargets.Remove(target);
    }

    private void OnDisable()
    {
        touchingTargets.Clear();
    }
}
