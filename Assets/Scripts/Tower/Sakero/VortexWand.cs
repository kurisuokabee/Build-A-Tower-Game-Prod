
using UnityEngine;

public class VortexWand : SakeroWand
{
    [Header("Vortex")]
    [SerializeField] private VortexEffect vortexPrefab;

    [Header("Serenity")]
    [SerializeField] private float serenityVortexDuration = 3f;

    private VortexEffect cursorVortex;
    private float serenityTimer;

    private bool hasLeftUpgrade;
    private bool hasRightUpgrade;

    public void SetLeftUpgradeEnabled(bool enabled)
    {
        hasLeftUpgrade = enabled;
    }

    public void SetRightUpgradeEnabled(bool enabled)
    {
        hasRightUpgrade = enabled;
    }

    protected override void Update()
    {
        if (mainCamera == null)
            return;

        bool serenityActive =
            TowerUtilities.Instance != null &&
            TowerUtilities.Instance.IsSerenityActive;

        if (serenityActive)
        {
            DestroyCursorVortex();
            serenityTimer += Time.deltaTime;

            float interval =
                1f / Mathf.Max(0.01f, CurrentAttacksPerSecond);

            if (serenityTimer >= interval)
            {
                serenityTimer -= interval;
                AttackAllTargetsWithVortex();
            }

            return;
        }

        serenityTimer = 0f;

        AttackableHealth target =
            GetHoveredEnemy(GetMouseWorldPosition());

        if (target == null)
        {
            DestroyCursorVortex();
            return;
        }

        FollowCursorVortex();
    }

    private void AttackAllTargetsWithVortex()
    {
        var targets = AttackableTargetRegistry.Targets;

        if (targets == null)
            return;

        float rangeSquared = serenityRange * serenityRange;
        Vector2 sakeroPosition = transform.position;

        for (int i = targets.Count - 1; i >= 0; i--)
        {
            AttackableHealth target = targets[i];

            if (target == null ||
                !target.gameObject.activeInHierarchy ||
                !target.IsAlive)
                continue;

            Vector2 offset =
                (Vector2)target.transform.position - sakeroPosition;

            if (offset.sqrMagnitude > rangeSquared)
                continue;

            VortexEffect vortex = SpawnVortex(
                target.transform.position,
                true
            );

            if (vortex != null)
                vortex.SetTemporaryPersistent(serenityVortexDuration);
        }
    }

    protected override void AttackTarget(AttackableHealth target)
    {
        // Vortex positioning is handled in Update.
    }

    private void FollowCursorVortex()
    {
        if (cursorVortex == null)
        {
            cursorVortex = SpawnVortex(
                GetMouseWorldPosition(),
                true
            );
        }

        if (cursorVortex != null)
        {
            cursorVortex.transform.position =
                GetMouseWorldPosition();
        }
    }

    private void DestroyCursorVortex()
    {
        if (cursorVortex == null)
            return;

        ReturnVortex(cursorVortex.gameObject);
        cursorVortex = null;
    }

    public void CreateImpactVortex(Vector3 position)
    {
        VortexEffect vortex = SpawnVortex(position, false);

        if (vortex != null)
            vortex.PullTargetsOnce();
    }

    private VortexEffect SpawnVortex(
        Vector3 position,
        bool persistent)
    {
        if (vortexPrefab == null)
            return null;

        GameObject vortexObject =
            ObjectPoolManager.Instance != null
                ? ObjectPoolManager.Instance.GetObject(
                    vortexPrefab.gameObject,
                    position,
                    Quaternion.identity
                )
                : Instantiate(
                    vortexPrefab.gameObject,
                    position,
                    Quaternion.identity
                );

        if (vortexObject == null)
            return null;

        VortexEffect vortex =
            vortexObject.GetComponent<VortexEffect>();

        if (vortex == null)
        {
            Debug.LogWarning(
                "Vortex prefab is missing VortexEffect."
            );

            ReturnVortex(vortexObject);
            return null;
        }

        vortex.SetDamageStats(
            CurrentDamage,
            CurrentAttacksPerSecond
        );

        vortex.SetPersistent(persistent);
        vortex.SetUpgrades(hasLeftUpgrade, hasRightUpgrade);

        return vortex;
    }

    private void ReturnVortex(GameObject vortexObject)
    {
        if (vortexObject == null)
            return;

        if (ObjectPoolManager.Instance != null)
            ObjectPoolManager.Instance.ReturnObject(vortexObject);
        else
            Destroy(vortexObject);
    }

    protected override void OnDisable()
    {
        DestroyCursorVortex();
        base.OnDisable();
    }
}
