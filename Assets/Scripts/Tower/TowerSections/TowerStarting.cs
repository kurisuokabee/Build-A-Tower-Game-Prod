using System.Collections.Generic;
using UnityEngine;

public class TowerStarting : TowerSection
{
    public override void ApplyUpgrade()
    {
        throw new System.NotImplementedException();
    }

    protected override void Attack(List<Enemy> targets)
    {
    }

    protected override List<Enemy> FindTargets()
    {
        throw new System.NotImplementedException();
    }
}