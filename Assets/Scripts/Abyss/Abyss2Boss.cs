
using System.Collections.Generic;
using UnityEngine;

public class Abyss2Boss : BossController
{
    [Header("Formation Settings")]
    [SerializeField, Min(1)] private int clonesPerSide = 3;
    [SerializeField, Min(0.1f)] private float horizontalSpacing = 3f;
    [SerializeField, Min(0.1f)] private float verticalSpacing = 3f;

    [Header("Role Switching")]
    [SerializeField, Min(0.1f)] private float minSwitchInterval = 5f;
    [SerializeField, Min(0.1f)] private float maxSwitchInterval = 12f;

    private readonly List<Abyss2Clone> clones = new();

    private Abyss2Clone currentMainBoss;
    private float switchTimer;
    private bool hasInitialized;
    private bool isCloneInstance;
    private Transform towerCenter;

    public Abyss2Clone CurrentMainBoss => currentMainBoss;
    public AttackableHealth SharedHealth => Health;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (isCloneInstance)
            return;

        clones.Clear();
        currentMainBoss = null;
        switchTimer = 0f;
        hasInitialized = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (!isCloneInstance)
            ClearClones();
    }

    private void Update()
    {
        if (isCloneInstance || IsDefeated || clones.Count <= 1)
            return;

        switchTimer -= Time.deltaTime;

        if (switchTimer <= 0f)
            SwitchMainBoss();
    }

    public override void Initialize(BossData data)
    {
        base.Initialize(data);

        if (isCloneInstance || hasInitialized || data == null)
            return;

        hasInitialized = true;

        SpawnCharacters();
    }

    private void SpawnCharacters()
    {
        Abyss2Clone original = GetComponent<Abyss2Clone>();

        if (original == null)
        {
            Debug.LogError("Boss prefab is missing Abyss2Clone!");
            return;
        }

        TowerBuilding tower = TowerBuilding.Instance;

        if (tower == null || tower.StartingBase == null)
        {
            Debug.LogError("Starting tower reference is missing!");
            return;
        }

        towerCenter = tower.StartingBase.transform;

        // Position and register the original boss.
        transform.position = GetSpawnPosition(-1, 0);

        original.Initialize(this, true);
        clones.Add(original);

        Abyss2BossMovement originalMovement =
            GetComponent<Abyss2BossMovement>();

        if (originalMovement != null)
            originalMovement.InitializeMovement();

        // Spawn the remaining characters.
        for (int i = 1; i < clonesPerSide * 2; i++)
        {
            int side = i < clonesPerSide ? -1 : 1;
            int index = i < clonesPerSide ? i : i - clonesPerSide;

            Vector3 spawnPosition = GetSpawnPosition(side, index);

            GameObject cloneObject = Instantiate(
                gameObject,
                spawnPosition,
                transform.rotation
            );

            Abyss2Boss cloneBoss =
                cloneObject.GetComponent<Abyss2Boss>();

            if (cloneBoss != null)
            {
                cloneBoss.isCloneInstance = true;
                cloneBoss.enabled = false;
            }

            Abyss2Clone clone =
                cloneObject.GetComponent<Abyss2Clone>();

            if (clone == null)
            {
                Debug.LogError("Spawned character is missing Abyss2Clone!");
                Destroy(cloneObject);
                continue;
            }

            clone.Initialize(this, false);
            clones.Add(clone);
        }

        Debug.Log($"Abyss 2 spawned {clones.Count} characters.");

        SwitchMainBoss();
    }

    private Vector3 GetSpawnPosition(int side, int index)
    {
        Vector3 center = towerCenter.position;

        float x = center.x + side * horizontalSpacing;
        float y = center.y + (1 - index) * verticalSpacing;

        return new Vector3(x, y, center.z);
    }

    private void SwitchMainBoss()
    {
        List<Abyss2Clone> candidates = new();

        foreach (Abyss2Clone clone in clones)
        {
            if (clone == null || clone == currentMainBoss)
                continue;

            candidates.Add(clone);
        }

        if (candidates.Count == 0)
            return;

        if (currentMainBoss != null)
            currentMainBoss.SetMainBoss(false);

        currentMainBoss = candidates[Random.Range(0, candidates.Count)];
        currentMainBoss.SetMainBoss(true);

        switchTimer = Random.Range(
            minSwitchInterval,
            Mathf.Max(minSwitchInterval, maxSwitchInterval)
        );

        Debug.Log("Main boss switched to: " + currentMainBoss.name);
    }

    private void ClearClones()
    {
        foreach (Abyss2Clone clone in clones)
        {
            if (clone != null && clone.gameObject != gameObject)
                Destroy(clone.gameObject);
        }

        clones.Clear();
        currentMainBoss = null;
    }

    protected override void OnDefeated()
    {
        ClearClones();
    }
}
