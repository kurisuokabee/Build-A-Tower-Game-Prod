using System;
using System.Collections.Generic;
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

    [Header("Level 4 Win Condition")]
    [SerializeField] private float level4TargetHeight = 50f;

    [Header("Build Limits")]
    [SerializeField] private int[] buildLimits = { 5, 10, 15 };

    [SerializeField] private Button bossEventButton;

    
    public event Action<int> OnAbyssLevelChanged;
    public event Action<float, float> OnBossTimerChanged; 
    public event Action OnBossEventStarted;
    public event Action<bool> OnBossEventEnded; 

    public bool IsBossEventActive { get; private set; }
    public bool IsLevel4HordeActive { get; private set; }

    private float bossTimer;
    private float bossTotalTime;
    private GameObject activeBossInstance;

    public event Action OnBuildLimitChanged;

    public int CurrentBuildLimit
    {
        get
        {
            if (currentAbyssLevel >= 4)
                return int.MaxValue;

            int index = currentAbyssLevel - 1;

            if (buildLimits == null || index < 0 || index >= buildLimits.Length)
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
    }

    private void Update()
    {
        if (IsBossEventActive)
        {
            bossTimer -= Time.deltaTime;
            OnBossTimerChanged?.Invoke(bossTimer, bossTotalTime);

            
            if (activeBossInstance == null || !activeBossInstance.activeInHierarchy)
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

        if (IsLevel4HordeActive && GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            // check level 4 wincon
            if (towerBuilding != null && towerBuilding.GetTopY() >= level4TargetHeight)
            {
                IsLevel4HordeActive = false;
                if (enemySpawner != null) enemySpawner.SetHordeActive(false);
                GameManager.Instance.WinGame();
            }
        }
    }

    private void InitializeCurrentLevel()
    {
        if (enemySpawner != null) enemySpawner.SetHordeActive(false);
        if (debrisSpawner != null) debrisSpawner.SetSpawningEnabled(true);

        if (currentAbyssLevel == 4)
        {
            StartLevel4Horde();
        }
    }

    public void StartBossEvent()
    {   
        if (!IsBuildLimitReached)
            return;

        if (IsBossEventActive || currentAbyssLevel > 3)
            return;

        BossData data = GetCurrentBossData();
        if (data == null || data.bossPrefab == null || ObjectPoolManager.Instance == null)
        {
            Debug.LogWarning("Cannot start boss event: Missing BossData or ObjectPoolManager.");
            return;
        }

        IsBossEventActive = true;
        RefreshBossButton();
        bossTotalTime = data.eventTimerLength;
        bossTimer = bossTotalTime;

        // stop and clear active debris
        if (debrisSpawner != null)
        {
            debrisSpawner.SetSpawningEnabled(false);
            debrisSpawner.ClearAllDebris();
        }

        // spawn boss using objectpoolmanager
        Vector3 spawnPos = new Vector3(0f, 3f, 0f);
        activeBossInstance = ObjectPoolManager.Instance.GetObject(data.bossPrefab, spawnPos, Quaternion.identity);

        if (activeBossInstance != null)
        {
            AttackableHealth health = activeBossInstance.GetComponent<AttackableHealth>();
            if (health != null)
            {
                health.InitializeHealth(data.MaxHealth);
            }

            EnemySetup setup = activeBossInstance.GetComponent<EnemySetup>();
            TowerSection target = towerBuilding != null && towerBuilding.Sections.Count > 0 ? towerBuilding.Sections[0] : null;
            if (setup != null)
            {
                setup.Initialize(EnemyAttack.AttackType.Melee, target, data.bossSprite);
            }
        }

        OnBossEventStarted?.Invoke();
        Debug.Log($"Boss Event started for Abyss Level {currentAbyssLevel}!");
    }

    private void EndBossEvent(bool success)
    {
        if (!IsBossEventActive) return;

        IsBossEventActive = false;

        if (activeBossInstance != null)
        {
            if (activeBossInstance.activeInHierarchy)
            {
                PooledObject pooled = activeBossInstance.GetComponent<PooledObject>();

                if (pooled != null)
                    pooled.ReturnToPool();
                else
                    ObjectPoolManager.Instance?.ReturnObject(activeBossInstance);
            }

            activeBossInstance = null;
        }

        if (success)
        {
            Debug.Log($"Boss for Abyss Level {currentAbyssLevel} defeated!");
            

            currentAbyssLevel++;
            OnAbyssLevelChanged?.Invoke(currentAbyssLevel);

            if (currentAbyssLevel == 4)
            {
                StartLevel4Horde();
            }
            else
            {
                if (currentAbyssLevel < 4 && debrisSpawner != null)
                {
                    debrisSpawner.RespawnInitialDebris();
                    debrisSpawner.SetSpawningEnabled(true);
                    OnBuildLimitChanged?.Invoke();
                    RefreshBossButton();
                }
            }
        }
        else
        {
            Debug.Log("Boss event timed out! Try again.");
            // Restore the initial debris batch after the boss event.   
            if (currentAbyssLevel < 4 && debrisSpawner != null)
            {
                debrisSpawner.RespawnInitialDebris();
                debrisSpawner.SetSpawningEnabled(true);
                OnBuildLimitChanged?.Invoke();
                RefreshBossButton();
            }
        }

        OnBossEventEnded?.Invoke(success);
    }

    private void StartLevel4Horde()
    {
        IsLevel4HordeActive = true;
        if (debrisSpawner != null) debrisSpawner.SetSpawningEnabled(true);
        if (enemySpawner != null) enemySpawner.SetHordeActive(true);
        Debug.Log("Abyss Level 4 Endless Horde started!");
    }

    private BossData GetCurrentBossData()
    {
        int index = currentAbyssLevel - 1;
        if (bossDatas != null && index >= 0 && index < bossDatas.Length)
            return bossDatas[index];
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
        if (PlayerEconomy.Instance == null || damageAmount <= 0) return;

        
        int currentBlocks = PlayerEconomy.Instance.Blocks;
        int removeAmount = Mathf.Min(currentBlocks, damageAmount);
        if (removeAmount > 0)
        {
            PlayerEconomy.Instance.SpendBlocks(removeAmount);
        }
    }
}