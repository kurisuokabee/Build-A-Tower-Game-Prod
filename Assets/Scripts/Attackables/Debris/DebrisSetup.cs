using UnityEngine;

[RequireComponent(typeof(DebrisMovement))]
[RequireComponent(typeof(AttackableHealth))]
[RequireComponent(typeof(AttackableReward))]
[RequireComponent(typeof(Debris))]
public class DebrisSetup : MonoBehaviour
{
    public void Initialize(DebrisData data)
    {
        if (data == null) return;

        // Set sprite
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null && data.sprite != null)
        {
            sr.sprite = data.sprite;
        }

        // Setup fall speed
        float speed = Random.Range(data.MinFallSpeed, data.MaxFallSpeed);
        GetComponent<DebrisMovement>().Initialize(speed);

        // Setup health
        AttackableHealth health = GetComponent<AttackableHealth>();
        if (health != null)
        {
            health.InitializeHealth(data.MaxHealth);
        }

        AttackableReward reward = GetComponent<AttackableReward>();
        if (reward != null)
        {
            reward.Initialize(
                data.BaseExp,
                data.BaseBlocks,
                data.UpgradePointChance
            );
        }
    }
}