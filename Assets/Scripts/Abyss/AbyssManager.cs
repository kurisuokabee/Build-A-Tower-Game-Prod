using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbyssManager : MonoBehaviour
{
    public static AbyssManager Instance { get; private set; }

    [Header("Abyss Progression")]
    [SerializeField, Range(1, 4)] private int currentAbyssLevel = 1;
    [SerializeField] private BossData[] bossDatas;

    [Header("References")]
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private DebrisSpawner debrisSpawner;
    [SerializeField] private TowerBuilding towerBuilding;
    [SerializeField] private BossHealthBar bossHealthBar;
    [SerializeField] private TextMeshProUGUI bossNameText;

    [Header("Level 4 Win Condition")]
    [SerializeField] private float level4TargetHeight = 50f;

    [Header("Build Limits")]
    [SerializeField] private int[] buildLimits = { 5, 10, 15 };

    [Header("Boss Event UI")]
    [SerializeField] private Button bossEventButton;

    [Header("Boss Spawn Settings")]
    [SerializeField] private float bossSpawnHeightOffset = 3f;

    public event Action<int> OnAbyssLevelChanged;
    public event Action<float, float> OnBossTimerChanged;
    public event Action OnBossEventStarted;
    public event Action<bool> OnBossEventEnded;
    public event Action OnBuildLimitChanged;

    public bool IsBossEventActive { get; private set; }
    public bool IsLevel4HordeActive { get; private set; }

    private float bossTimer;
    private float bossTotalTime;
    private GameObject activeBossInstance;

    public int CurrentBuildLimit
    {
        get
        {
            if (currentAbyssLevel >= 4)
                return int.MaxValue;

            int index = currentAbyssLevel - 1;

            if (buildLimits == null ||
                index < 0 ||
                index >= buildLimits.Length)
                return 5;

            return buildLimits[index];
        }
    }

    public bool IsBuildLimitReached =>
        towerBuilding != null &&
        towerBuilding.Sections.Count >= CurrentBuildLimit &&
        currentAbyssLevel < 4;

    public bool CanBuildMoreSections =>
        currentAbyssLevel >= 4 ||
        towerBuilding == null ||
        towerBuilding.Sections.Count < CurrentBuildLimit;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (bossEventButton != null)
            bossEventButton.onClick.AddListener(StartBossEvent);
    }

    private void OnDisable()
    {
        if (bossEventButton != null)
            bossEventButton.onClick.RemoveListener(StartBossEvent);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void RefreshBossButton()
    {
        if (bossEventButton == null)
            return;

        bool shouldShow =
            IsBuildLimitReached &&
            !IsBossEventActive;

        bossEventButton.gameObject.SetActive(shouldShow);
    }

    public void RefreshProgressUI()
    {
        OnBuildLimitChanged?.Invoke();
        RefreshBossButton();
    }

    private void Start()
    {
        InitializeCurrentLevel();
        RefreshProgressUI();

        if (bossHealthBar != null)
            bossHealthBar.Hide();
    }

    private void Update()
    {
        if (IsBossEventActive)
        {
            bossTimer -= Time.deltaTime;
            OnBossTimerChanged?.Invoke(bossTimer, bossTotalTime);

            if (activeBossInstance == null ||
                !activeBossInstance.activeInHierarchy)
            {
                EndBossEvent(true);
                return;
            }

            if (bossTimer <= 0f)
            {
                EndBossEvent(false);
                return;
            }
        }

        if (IsLevel4HordeActive &&
            GameManager.Instance != null &&
            GameManager.Instance.CurrentState == GameState.Playing)
        {
            if (towerBuilding != null &&
                towerBuilding.GetTopY() >= level4TargetHeight)
            {
                IsLevel4HordeActive = false;

                if (enemySpawner != null)
                    enemySpawner.SetHordeActive(false);

                GameManager.Instance.WinGame();
            }
        }
    }

    private void InitializeCurrentLevel()
    {
        if (enemySpawner != null)
            enemySpawner.SetHordeActive(false);

        if (debrisSpawner != null)
            debrisSpawner.SetSpawningEnabled(true);

        if (currentAbyssLevel == 4)
            StartLevel4Horde();
    }

    public void StartBossEvent()
    {
        if (!IsBuildLimitReached)
            return;

        if (IsBossEventActive || currentAbyssLevel > 3)
            return;

        BossData data = GetCurrentBossData();

        if (data == null ||
            data.bossPrefab == null ||
            ObjectPoolManager.Instance == null)
        {
            Debug.LogWarning(
                "Cannot start boss event: Missing BossData or ObjectPoolManager."
            );
            return;
        }

        // Initialize boss event.
        bossTotalTime = data.eventTimerLength;
        bossTimer = bossTotalTime;
        IsBossEventActive = true;

        RefreshBossButton();

        // Stop and clear active debris.
        if (debrisSpawner != null)
        {
            debrisSpawner.SetSpawningEnabled(false);
            debrisSpawner.ClearAllDebris();
        }

        // Spawn boss.
        Vector3 spawnPos = GetBossSpawnPosition();

        activeBossInstance = ObjectPoolManager.Instance.GetObject(
            data.bossPrefab,
            spawnPos,
            Quaternion.identity
        );

        if (activeBossInstance == null)
        {
            Debug.LogWarning("Failed to spawn boss.");

            IsBossEventActive = false;
            RefreshBossButton();
            RestoreDebris();

            return;
        }

        // Initialize boss controller.
        BossController bossController =
            activeBossInstance.GetComponent<BossController>();

        if (bossController == null)
        {
            Debug.LogWarning("Boss prefab is missing BossController.");

            ObjectPoolManager.Instance.ReturnObject(activeBossInstance);
            activeBossInstance = null;

            IsBossEventActive = false;
            RefreshBossButton();
            RestoreDebris();

            return;
        }

        bossController.Initialize(data);

        // Display boss health bar.
        if (bossHealthBar != null)
            bossHealthBar.SetBoss(bossController);

        bossNameText.text = data.bossName;

        OnBossEventStarted?.Invoke();

        Debug.Log(
            $"Boss Event started for Abyss Level {currentAbyssLevel}!"
        );
    }

    private void EndBossEvent(bool success)
    {
        if (!IsBossEventActive)
            return;

        IsBossEventActive = false;

        // Hide boss health bar.
        if (bossHealthBar != null)
            bossHealthBar.Hide();

        // Return boss to pool.
        if (activeBossInstance != null)
        {
            if (activeBossInstance.activeInHierarchy)
            {
                PooledObject pooled =
                    activeBossInstance.GetComponent<PooledObject>();

                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    ObjectPoolManager.Instance?.ReturnObject(activeBossInstance);
            }

            activeBossInstance = null;
        }

        if (success)
        {
            Debug.Log(
                $"Boss for Abyss Level {currentAbyssLevel} defeated!"
            );

            currentAbyssLevel++;
            OnAbyssLevelChanged?.Invoke(currentAbyssLevel);

            if (currentAbyssLevel == 4)
            {
                StartLevel4Horde();
            }
            else
            {
                RestoreDebris();
                OnBuildLimitChanged?.Invoke();
                RefreshBossButton();
            }
        }
        else
        {
            Debug.Log("Boss event timed out! Try again.");

            RestoreDebris();
            OnBuildLimitChanged?.Invoke();
            RefreshBossButton();
        }

        OnBossEventEnded?.Invoke(success);
    }

    private void RestoreDebris()
    {
        if (currentAbyssLevel >= 4 || debrisSpawner == null)
            return;

        debrisSpawner.RespawnInitialDebris();
        debrisSpawner.SetSpawningEnabled(true);
    }

    private void StartLevel4Horde()
    {
        IsLevel4HordeActive = true;

        if (debrisSpawner != null)
            debrisSpawner.SetSpawningEnabled(true);

        if (enemySpawner != null)
            enemySpawner.SetHordeActive(true);

        Debug.Log("Abyss Level 4 Endless Horde started!");
    }

    private BossData GetCurrentBossData()
    {
        int index = currentAbyssLevel - 1;

        if (bossDatas != null &&
            index >= 0 &&
            index < bossDatas.Length)
        {
            return bossDatas[index];
        }

        return null;
    }

    public float GetCurrentDifficulty()
    {
        if (towerBuilding == null || towerBuilding.Sections == null)
            return 1f;

        return Mathf.Max(1f, towerBuilding.Sections.Count);
    }

    public void DamageTowerBlocks(int damageAmount)
    {
        if (PlayerEconomy.Instance == null || damageAmount <= 0)
            return;

        int currentBlocks = PlayerEconomy.Instance.Blocks;
        int removeAmount = Mathf.Min(currentBlocks, damageAmount);

        if (removeAmount > 0)
            PlayerEconomy.Instance.SpendBlocks(removeAmount);
    }

    private Vector3 GetBossSpawnPosition()
    {
        if (towerBuilding == null ||
            towerBuilding.StartingBase == null)
        {
            return new Vector3(0f, 3f, 0f);
        }

        Vector3 towerPosition =
            towerBuilding.StartingBase.transform.position;

        return new Vector3(
            towerPosition.x,
            towerBuilding.GetTopY() + bossSpawnHeightOffset,
            towerPosition.z
        );
    }
}
