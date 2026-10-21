
using System.Collections.Generic;
using UnityEngine;

public class TowerEnemyFinder : MonoBehaviour
{
    public Enemy FindNearestEnemy(float searchRange)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy nearest = null;
        float nearestDistance = searchRange * searchRange;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance =
                (transform.position - enemy.transform.position).sqrMagnitude;

            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = enemy;
            }
        }

        return nearest;
    }

    public List<Enemy> FindNearestEnemies(float searchRange, int count)
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        List<Enemy> targets = new List<Enemy>();

        float rangeSquared = searchRange * searchRange;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance =
                (transform.position - enemy.transform.position).sqrMagnitude;

            if (distance <= rangeSquared)
                targets.Add(enemy);
        }

        targets.Sort((a, b) =>
        {
            float distanceA =
                (a.transform.position - transform.position).sqrMagnitude;
            float distanceB =
                (b.transform.position - transform.position).sqrMagnitude;

            return distanceA.CompareTo(distanceB);
        });

        if (targets.Count > count)
            targets.RemoveRange(count, targets.Count - count);

        return targets;
    }
}
