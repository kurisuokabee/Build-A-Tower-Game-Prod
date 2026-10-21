using System.Collections.Generic;
using UnityEngine;

public class TowerBuilding : MonoBehaviour
{   
    public static TowerBuilding Instance { get; private set; }

    [Header("Tower Sections")]
    [SerializeField] private TowerSection startingBase;
    [SerializeField] private TowerHealth towerHealth;

    private readonly List<TowerSection> sections = new();

    public IReadOnlyList<TowerSection> Sections => sections;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RebuildTower();
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

        if (!section.CanAfford)
        {
            Debug.Log("Not enough Blocks!");
            return false;
        }

        // Charge only when a new section is inserted.
        if (economy.Blocks < section.BlockCost)
            return false;

        index = Mathf.Clamp(index, 0, sections.Count);

        economy.SpendBlocks(section.BlockCost);

        section.transform.SetParent(transform);
        sections.Insert(index, section);

        RebuildTower();

        return true;
    }
    private void RebuildTower()
    {
        float currentY = 0f;

        // Position sections from bottom to top.
        for (int i = sections.Count - 1; i >= 0; i--)
        {
            TowerSection section = sections[i];

            if (section == null)
                continue;

            section.transform.localPosition =
                new Vector3(0f, currentY + section.height / 2f, 0f);

            currentY += section.height;
        }

        // Keep the starting base on top.
        if (startingBase != null)
        {
            startingBase.transform.SetParent(transform);
            startingBase.transform.localPosition =
                new Vector3(0f, currentY + startingBase.height / 2f, 0f);
        }

        if (towerHealth != null)
            towerHealth.UpdateSectionCount(sections.Count);
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
    }

    public bool HasSection(TowerUpgradeID upgradeID)
    {
        foreach (TowerSection section in sections)
        {
            if (section != null && section.Data != null &&
                section.Data.towerUpgradeID == upgradeID)
            {
                return true;
            }
        }

        return false;
    }
}
