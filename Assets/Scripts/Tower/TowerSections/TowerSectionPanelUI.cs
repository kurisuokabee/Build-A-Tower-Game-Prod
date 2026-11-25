
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSectionPanelUI : MonoBehaviour
{
    [Header("Views")]
    [SerializeField] private GameObject towerSectionsView;
    [SerializeField] private GameObject towerStatsView;

    [Header("Switch Button")]
    [SerializeField] private Button switchViewButton;
    [SerializeField] private TMP_Text switchButtonText;

    private bool showingStats;

    private void Start()
    {
        if (switchViewButton != null)
            switchViewButton.onClick.AddListener(ToggleView);

        ShowSectionsView();
    }

    private void OnDestroy()
    {
        if (switchViewButton != null)
            switchViewButton.onClick.RemoveListener(ToggleView);
    }

    private void ToggleView()
    {
        if (showingStats)
            ShowSectionsView();
        else
            ShowStatsView();
    }

    private void ShowSectionsView()
    {
        showingStats = false;

        if (towerSectionsView != null)
            towerSectionsView.SetActive(true);

        if (towerStatsView != null)
            towerStatsView.SetActive(false);

        if (switchButtonText != null)
            switchButtonText.text = "View Stats";
    }

    private void ShowStatsView()
    {
        showingStats = true;

        if (towerSectionsView != null)
            towerSectionsView.SetActive(false);

        if (towerStatsView != null)
            towerStatsView.SetActive(true);

        if (switchButtonText != null)
            switchButtonText.text = "View Sections";
    }
}
