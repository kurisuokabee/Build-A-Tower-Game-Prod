using UnityEngine;

public class MockUpgradeTester : MonoBehaviour
{
    [SerializeField] private OrbLaserCombat laserCombat;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            if (laserCombat != null)
            {
                laserCombat.ApplyUpgrade();
            }
        }
    }
}