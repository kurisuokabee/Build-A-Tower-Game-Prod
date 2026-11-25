using UnityEngine;

[RequireComponent(typeof(DebrisMovement))]
[RequireComponent(typeof(AttackableHealth))]
[RequireComponent(typeof(AttackableReward))]
[RequireComponent(typeof(Debris))]
public class DebrisSetup : MonoBehaviour
{
    public void Initialize(DebrisData data, float healthMultiplier)
    {
        if (data == null)
            return;

        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();

        if (sr != null && data.sprite != null)
            sr.sprite = data.sprite;

        float speed = Random.Range(
            data.MinFallSpeed,
            data.MaxFallSpeed
        );

        GetComponent<DebrisMovement>().Initialize(speed);

        AttackableHealth health = GetComponent<AttackableHealth>();

        if (health != null)
        {
            float scaledHealth = data.MaxHealth * healthMultiplier;
            health.InitializeHealth(scaledHealth);
        }

        AttackableReward reward = GetComponent<AttackableReward>();

        if (reward != null)
        {
            reward.Initialize(data.type, data.BaseReward);
        }
    }
}
