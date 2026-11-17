using UnityEngine;

public class Abyss3Minion : MonoBehaviour
{
    private Abyss3Boss boss;

    public void Initialize(Abyss3Boss bossController)
    {
        boss = bossController;
    }

    public void RegisterKill()
    {
        if (boss != null)
            boss.RegisterMinionKill();
    }

    private void OnDisable()
    {
        boss = null;
    }
}
