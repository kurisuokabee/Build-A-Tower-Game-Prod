using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class TowerSectionUI : MonoBehaviour
{
    [Header("Section")]
    [SerializeField] private TowerSection sectionPrefab;
    [SerializeField] private TowerSectionGhost ghostPrefab;
    [SerializeField] private TowerBuilding tower;

    [Header("UI")]
    [SerializeField] private Button sectionButton;
    [SerializeField] private TMP_Text sectionNameText;
    [SerializeField] private TMP_Text blockCostText;
    [SerializeField] private TMP_Text levelRequirementText;

    private TowerSectionGhost draggedGhost;
    private bool isPlacing;
    private bool canPlace;

    public static bool IsHoldingSection { get; private set; }
    public static bool IgnoreRelocatorClick { get; private set; }

    private PlayerEconomy economy;

    private void Start()
    {
        economy = PlayerEconomy.Instance;

        UpdateSectionUI();

        if (economy != null)
            economy.OnResourcesChanged += UpdateSectionUI;

        if (sectionButton != null)
            sectionButton.onClick.AddListener(OnSectionButtonClicked);
    }

    private void OnDestroy()
    {
        if (economy != null)
            economy.OnResourcesChanged -= UpdateSectionUI;
    }

    private void Update()
    {
        if (economy != PlayerEconomy.Instance)
        {
            if (economy != null)
                economy.OnResourcesChanged -= UpdateSectionUI;

            economy = PlayerEconomy.Instance;

            if (economy != null)
                economy.OnResourcesChanged += UpdateSectionUI;

            UpdateSectionUI();
        }

        if (!isPlacing || Mouse.current == null)
            return;

        Vector3 mousePosition = MousePosition();

        if (isCursorOverTower(mousePosition))
        {
            int index = tower.GetInsertionIndex(mousePosition.y);
            draggedGhost.transform.position =
                tower.GetInsertionPosition(index);
            draggedGhost.SetAlpha(0.5f);
        }
        else
        {
            draggedGhost.transform.position = mousePosition;
            draggedGhost.SetAlpha(1f);
        }

        if (!Mouse.current.leftButton.isPressed)
            canPlace = true;

        if (canPlace && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isCursorOverTower(mousePosition))
            {
                int index = tower.GetInsertionIndex(mousePosition.y);
                TowerSection newSection = Instantiate(sectionPrefab);

                if (!tower.InsertTowerSection(index, newSection))
                {
                    Destroy(newSection.gameObject);
                }
                else
                {
                    IgnoreRelocatorClick = true;
                }
            }

            CancelPlacement();
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
            CancelPlacement();
    }

    private void UpdateSectionUI()
    {
        if (sectionPrefab == null)
            return;

        if (sectionNameText != null)
            sectionNameText.text = sectionPrefab.name;

        if (blockCostText != null)
            blockCostText.text = $"{sectionPrefab.BlockCost} Blocks";

        if (levelRequirementText != null && !sectionPrefab.IsUnlocked)
        {
            levelRequirementText.text =
                $"Locked until Level {sectionPrefab.RequiredLevel}";
        }
        else 
        {
            levelRequirementText.text = "";
        }
            

        bool unlocked = sectionPrefab.IsUnlocked;

        if (sectionButton != null)
            sectionButton.interactable =
            sectionPrefab != null &&
            sectionPrefab.IsUnlocked &&
            sectionPrefab.CanAfford;
    }

    public void OnSectionButtonClicked()
    {
        if (isPlacing || sectionPrefab == null || ghostPrefab == null)
            return;

        if (economy == null || !sectionPrefab.IsUnlocked)
            return;

        if (!sectionPrefab.CanAfford)
        {
            Debug.Log("Not enough Blocks!");
            return;
        }

        draggedGhost = Instantiate(ghostPrefab);
        draggedGhost.SetSprite(sectionPrefab);
        draggedGhost.SetAlpha(1f);

        isPlacing = true;
        canPlace = false;
        IsHoldingSection = true;
    }

    private bool isCursorOverTower(Vector3 mousePosition)
    {
        if (tower == null || sectionPrefab == null)
            return false;

        float halfWidth = sectionPrefab.width / 2f;

        return mousePosition.x >= tower.transform.position.x - halfWidth &&
               mousePosition.x <= tower.transform.position.x + halfWidth &&
               mousePosition.y >= tower.GetBottomY() &&
               mousePosition.y <= tower.GetTopY();
    }

    private Vector3 MousePosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            Mouse.current.position.ReadValue()
        );

        mousePosition.z = 0f;
        return mousePosition;
    }

    private void CancelPlacement()
    {
        if (draggedGhost != null)
            Destroy(draggedGhost.gameObject);

        draggedGhost = null;
        isPlacing = false;
        canPlace = false;
        IsHoldingSection = false;
    }

    public static void ClearRelocatorClick()
    {
        IgnoreRelocatorClick = false;
    }
}