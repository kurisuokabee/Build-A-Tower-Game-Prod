using UnityEngine;

public class EnemyFinder : MonoBehaviour
{
    public Enemy FindNearestEnemy(float searchRange)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy nearest = null;
        float nearestDistance = searchRange;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            //EnemyHP enemyHP = enemy.GetComponent<EnemyHP>();

            // if (enemyHP == null || !enemyHP.IsAlive)
            //     continue;

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }
}