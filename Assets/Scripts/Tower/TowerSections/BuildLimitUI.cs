using UnityEngine;
using TMPro;

public class BuildLimitUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text buildLimitText;
    [SerializeField] private TowerBuilding tower;

    private AbyssManager abyssManager;

    private void Start()
    {
        if (tower == null)
            tower = TowerBuilding.Instance;

        abyssManager = AbyssManager.Instance;

        if (tower != null)
            tower.OnTowerSectionsChanged += UpdateUI;

        if (abyssManager != null)
            abyssManager.OnBuildLimitChanged += UpdateUI;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (tower != null)
            tower.OnTowerSectionsChanged -= UpdateUI;

        if (abyssManager != null)
            abyssManager.OnBuildLimitChanged -= UpdateUI;
    }

    public void UpdateUI()
    {
        if (buildLimitText == null)
            return;

        if (tower == null)
            tower = TowerBuilding.Instance;

        if (abyssManager == null)
            abyssManager = AbyssManager.Instance;

        if (tower == null || abyssManager == null)
            return;

        int currentSections = tower.Sections.Count;
        int limit = abyssManager.CurrentBuildLimit;

        if (limit == int.MaxValue)
            buildLimitText.text = $"Tower Sections: {currentSections} / No Limit";
        else
            buildLimitText.text = $"Tower Sections: {currentSections} / {limit}";
    }
}