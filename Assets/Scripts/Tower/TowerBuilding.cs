
using System.Collections.Generic;
using UnityEngine;

public class TowerBuilding : MonoBehaviour
{
    public static TowerBuilding Instance { get; private set; }

    [Header("Tower Sections")]
    [SerializeField] private TowerSection startingBase;

    [Header("Build Cost Scaling")]
    [SerializeField] private int buildCostIncreasePerSection = 25;

    private Vector3 startingPosition;

    private readonly List<TowerSection> sections = new();
    private int successfulBuilds;

    public IReadOnlyList<TowerSection> Sections => sections;
    public int SuccessfulBuilds => successfulBuilds;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        startingPosition = transform.position;
    }

    private void Start()
    {
        RebuildTower();
        RefreshTemporaryUpgrades();
    }

    public int GetCurrentBuildCost(TowerSection section)
    {
        if (section == null)
            return 0;

        return section.BlockCost +
            buildCostIncreasePerSection * successfulBuilds;
    }

    public bool BuildTowerSection(TowerSection section)
    {
        if (section == null)
            return false;

        return InsertTowerSection(0, section);
    }

    public bool InsertTowerSection(int index, TowerSection section)
    {
        if (section == null)
            return false;

        PlayerEconomy economy = PlayerEconomy.Instance;

        if (economy == null)
            return false;

        if (!section.IsUnlocked)
        {
            Debug.Log("This section is locked!");
            return false;
        }

        int buildCost = GetCurrentBuildCost(section);

        if (economy.Blocks < buildCost)
        {
            Debug.Log($"Not enough Blocks! Cost: {buildCost}");
            return false;
        }

        index = Mathf.Clamp(index, 0, sections.Count);

        if (!economy.SpendBlocks(buildCost))
            return false;

        section.transform.SetParent(transform);
        sections.Insert(index, section);

        successfulBuilds++;

        RebuildTower();
        RefreshTemporaryUpgrades();

        return true;
    }

    
    
    
    private void RebuildTower()
    {
        if (startingBase == null)
            return;

        float totalHeight = 0f;

        foreach (TowerSection section in sections)
        {
            if (section != null)
                totalHeight += section.height;
        }

        // Move the tower upward from its original position.
        transform.position = startingPosition + new Vector3(
            0f,
            totalHeight,
            0f
        );

        startingBase.transform.SetParent(transform);

        startingBase.transform.localPosition = new Vector3(
            0f,
            startingBase.height / 2f,
            0f
        );

        // Place sections directly below the starting tower.
        float currentY = 0f;

        for (int i = 0; i < sections.Count; i++)
        {
            TowerSection section = sections[i];

            if (section == null)
                continue;

            currentY -= section.height / 2f;

            section.transform.localPosition = new Vector3(
                0f,
                currentY,
                0f
            );

            currentY -= section.height / 2f;
        }
    }




    public int GetInsertionIndex(float mouseY)
    {
        for (int i = 0; i < sections.Count; i++)
        {
            TowerSection section = sections[i];

            if (section == null)
                continue;

            float sectionTop =
                section.transform.position.y + section.height / 2f;

            if (mouseY > sectionTop)
                return i;
        }

        return sections.Count;
    }

    public Vector3 GetInsertionPosition(int index)
    {
        float y = 0f;

        for (int i = sections.Count - 1; i >= index; i--)
        {
            if (sections[i] != null)
                y += sections[i].height;
        }

        return transform.position + new Vector3(0f, y, 0f);
    }

    public float GetTopY()
    {
        if (startingBase == null)
            return transform.position.y;

        return startingBase.transform.position.y
            + startingBase.height / 2f;
    }

    public float GetBottomY()
    {
        return transform.position.y;
    }

    public TowerSection GetSectionAtPosition(
        Vector3 position,
        TowerSection ignoreSection = null)
    {
        foreach (TowerSection section in sections)
        {
            if (section == null || section == ignoreSection)
                continue;

            Vector3 sectionPosition = section.transform.position;

            bool insideX =
                Mathf.Abs(position.x - sectionPosition.x)
                <= section.width / 2f;

            bool insideY =
                Mathf.Abs(position.y - sectionPosition.y)
                <= section.height / 2f;

            if (insideX && insideY)
                return section;
        }

        return null;
    }

    public void SwapTowerSections(
        TowerSection first,
        TowerSection second)
    {
        int firstIndex = sections.IndexOf(first);
        int secondIndex = sections.IndexOf(second);

        if (firstIndex < 0 || secondIndex < 0)
            return;

        sections[firstIndex] = second;
        sections[secondIndex] = first;

        RebuildTower();
        RefreshTemporaryUpgrades();
    }

    public bool HasSection(TowerUpgradeID upgradeID)
    {
        foreach (TowerSection section in sections)
        {
            if (section != null &&
                section.Data != null &&
                section.Data.towerUpgradeID == upgradeID)
            {
                return true;
            }
        }

        return false;
    }

    public void TriggerAdjacentSections(TowerSection source)
    {
        if (source == null)
            return;

        int index = sections.IndexOf(source);

        if (index < 0)
            return;

        // Section directly below.
        if (index > 0)
        {
            TowerSection below = sections[index - 1];

            if (below != null)
                below.TriggerAttackImmediately();
        }

        // Section directly above.
        if (index < sections.Count - 1)
        {
            TowerSection above = sections[index + 1];

            if (above != null)
                above.TriggerAttackImmediately();
        }
    }

    
    
    public void RefreshTemporaryUpgrades()
    {
        int[] bonuses = new int[sections.Count];

        for (int i = 0; i < sections.Count; i++)
        {
            TowerSection section = sections[i];

            if (section == null || !section.isActiveAndEnabled)
                continue;

            // Upgrading Spear behavior.
            if (section is TowerSpear spear &&
                spear.GrantsAdjacentUpgrade)
            {
                if (i > 0 &&
                    sections[i - 1] != null &&
                    sections[i - 1].isActiveAndEnabled)
                {
                    bonuses[i - 1]++;
                }

                if (i < sections.Count - 1 &&
                    sections[i + 1] != null &&
                    sections[i + 1].isActiveAndEnabled)
                {
                    bonuses[i + 1]++;
                }
            }

            // Alliance behavior.
            if (section is TowerFlail flail &&
                flail.GrantsAlliance &&
                i > 0 &&
                i < sections.Count - 1)
            {
                TowerSection below = sections[i - 1];
                TowerSection above = sections[i + 1];

                if (IsAllianceNeighbor(below) &&
                    IsAllianceNeighbor(above))
                {
                    bonuses[i] += 2;
                    bonuses[i - 1] += 2;
                    bonuses[i + 1] += 2;
                }
            }
        }

        for (int i = 0; i < sections.Count; i++)
        {
            if (sections[i] != null)
                sections[i].SetTemporaryLevelBonus(bonuses[i]);
        }
    }

    private bool IsAllianceNeighbor(TowerSection section)
    {
        if (section == null || !section.isActiveAndEnabled ||
            section.Data == null)
        {
            return false;
        }

        TowerUpgradeID id = section.Data.towerUpgradeID;

        return id == TowerUpgradeID.Spear ||
            id == TowerUpgradeID.Flail;
    }


}
