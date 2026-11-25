
using UnityEngine;

public class TowerImpactEffect : MonoBehaviour
{
    public static TowerImpactEffect Instance { get; private set; }

    [Header("Ice Wand")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 3f;

    private TowerBuilding building;

    private IceWand iceWand;
    private ElectricWand electricWand;
    private VortexWand vortexWand;
    private MeteorWand meteorWand;

    private void Awake()
    {
        Instance = this;
        building = TowerBuilding.Instance;

        RefreshWands();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void RefreshWands()
    {
        if (building == null)
            building = TowerBuilding.Instance;

        if (building == null || building.StartingBase == null)
            return;

        iceWand = building.StartingBase
            .GetComponentInChildren<IceWand>();

        electricWand = building.StartingBase
            .GetComponentInChildren<ElectricWand>();

        vortexWand = building.StartingBase
            .GetComponentInChildren<VortexWand>();

        meteorWand = building.StartingBase
            .GetComponentInChildren<MeteorWand>();
    }

    public void HandleImpact(
        TowerSection section,
        AttackableHealth target)
    {   
        if (section == null ||
            target == null ||
            !target.IsAlive)
            return;

        if (building == null)
            building = TowerBuilding.Instance;

        if (building == null ||
            building.GetSectionUnderSakero() != section)
            return;

        RefreshWands();

        if (iceWand != null)
        {
            AttackableSlow slow =
                target.GetComponent<AttackableSlow>();

            if (slow != null)
                slow.ApplySlow(slowMultiplier, slowDuration);
        }

        if (electricWand != null)
        {
            electricWand.TriggerChainAttack(
                target,
                section.EffectiveDamage
            );
        }

        if (vortexWand != null)
        {
            vortexWand.CreateImpactVortex(
                target.transform.position
            );
        }

        if (meteorWand != null)
        {
            meteorWand.CreateImpactMeteor(target);
        }
    }
}
