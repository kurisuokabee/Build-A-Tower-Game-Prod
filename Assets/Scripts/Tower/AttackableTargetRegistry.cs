using System.Collections.Generic;
using UnityEngine;

public static class AttackableTargetRegistry
{
    private static readonly List<AttackableHealth> targets = new();

    public static IReadOnlyList<AttackableHealth> Targets => targets;

    public static void Register(AttackableHealth target)
    {
        if (target == null || targets.Contains(target))
            return;

        targets.Add(target);
    }

    public static void Unregister(AttackableHealth target)
    {
        if (ReferenceEquals(target, null))
            return;

        targets.Remove(target);
    }

    public static void Clear()
    {
        targets.Clear();
    }
}
