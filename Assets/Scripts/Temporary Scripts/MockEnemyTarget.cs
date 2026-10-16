using UnityEngine;

public class MockEnemyTarget : MonoBehaviour
{
    [SerializeField] private float health = 50f;

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0f)
        {
            gameObject.SetActive(false);
        }
    }
}