
using System.Collections.Generic;
using UnityEngine;

public class TowerEnemyFinder : MonoBehaviour
{
    private static readonly Dictionary<Enemy, TowerEnemyFinder> reservations =
        new Dictionary<Enemy, TowerEnemyFinder>();

    private List<Enemy> currentTargets = new List<Enemy>();

    public List<Enemy> FindNearestEnemies(float searchRange, int count)
    {
        ReleaseTargets();

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        List<Enemy> availableTargets = new List<Enemy>();
        List<Enemy> fallbackTargets = new List<Enemy>();

        float rangeSquared = searchRange * searchRange;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance =
                (enemy.transform.position - transform.position).sqrMagnitude;

            if (distance > rangeSquared)
                continue;

            fallbackTargets.Add(enemy);

            if (!reservations.ContainsKey(enemy))
                availableTargets.Add(enemy);
        }

        availableTargets.Sort(CompareDistance);
        fallbackTargets.Sort(CompareDistance);

        List<Enemy> targets = new List<Enemy>();

        // Prefer enemies that aren't targeted by another section.
        foreach (Enemy enemy in availableTargets)
        {
            if (targets.Count >= count)
                break;

            targets.Add(enemy);
        }

        // If there aren't enough enemies, allow shared targets.
        foreach (Enemy enemy in fallbackTargets)
        {
            if (targets.Count >= count)
                break;

            if (!targets.Contains(enemy))
                targets.Add(enemy);
        }

        foreach (Enemy enemy in targets)
        {
            reservations[enemy] = this;
            currentTargets.Add(enemy);
        }

        return targets;
    }

    public Enemy FindNearestEnemy(float searchRange)
    {
        List<Enemy> targets = FindNearestEnemies(searchRange, 1);
        return targets.Count > 0 ? targets[0] : null;
    }

    private int CompareDistance(Enemy a, Enemy b)
    {
        float distanceA =
            (a.transform.position - transform.position).sqrMagnitude;

        float distanceB =
            (b.transform.position - transform.position).sqrMagnitude;

        return distanceA.CompareTo(distanceB);
    }

    private void ReleaseTargets()
    {
        foreach (Enemy enemy in currentTargets)
        {
            if (enemy != null &&
                reservations.TryGetValue(enemy, out TowerEnemyFinder owner) &&
                owner == this)
            {
                reservations.Remove(enemy);
            }
        }

        currentTargets.Clear();
    }

    private void OnDisable()
    {
        ReleaseTargets();
    }
}
