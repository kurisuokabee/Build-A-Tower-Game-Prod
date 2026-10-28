using System.Collections.Generic;
using UnityEngine;

public static class AttackableTargetFinder
{
    private static readonly List<AttackableHealth> results = new();
    private static readonly List<float> distances = new();

    public static IReadOnlyList<AttackableHealth> FindNearestTargets(
        Vector2 position,
        int count,
        float range = Mathf.Infinity)
    {
        results.Clear();
        distances.Clear();

        if (count <= 0 || range < 0f)
            return results;

        IReadOnlyList<AttackableHealth> targets =
            AttackableTargetRegistry.Targets;

        float rangeSquared = range * range;

        for (int i = 0; i < targets.Count; i++)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.isActiveAndEnabled ||
                !target.IsAlive)
            {
                continue;
            }

            float distanceSquared =
                ((Vector2)target.transform.position - position).sqrMagnitude;

            if (distanceSquared > rangeSquared)
                continue;

            int insertIndex = results.Count;

            for (int j = 0; j < results.Count; j++)
            {
                if (distanceSquared < distances[j])
                {
                    insertIndex = j;
                    break;
                }
            }

            if (insertIndex >= count)
                continue;

            results.Insert(insertIndex, target);
            distances.Insert(insertIndex, distanceSquared);

            if (results.Count > count)
            {
                results.RemoveAt(results.Count - 1);
                distances.RemoveAt(distances.Count - 1);
            }
        }

        return results;
    }

    public static AttackableHealth FindNearestTarget(
        Vector2 position,
        float range = Mathf.Infinity)
    {
        IReadOnlyList<AttackableHealth> targets =
            FindNearestTargets(position, 1, range);

        return targets.Count > 0 ? targets[0] : null;
    }
}
