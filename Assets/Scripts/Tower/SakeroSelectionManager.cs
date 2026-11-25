
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SakeroSelectionManager : MonoBehaviour
{
    [System.Serializable]
    public class SakeroOption
    {
        public string sakeroName;
        public Button selectionButton;
        public GameObject selectionHighlight;
    }

    [Header("Sakero Options")]
    [SerializeField] private SakeroOption[] sakeroOptions;

    [Header("Start Game")]
    [SerializeField] private Button startGameButton;
    [SerializeField] private string gameplaySceneName = "TowerBuilding";

    private const string SelectedSakeroKey = "SelectedSakero";

    private int selectedIndex;

    private void Awake()
    {
        for (int i = 0; i < sakeroOptions.Length; i++)
        {
            int index = i;

            if (sakeroOptions[i].selectionButton != null)
            {
                sakeroOptions[i].selectionButton.onClick.AddListener(
                    () => SelectSakero(index)
                );
            }
        }

        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
    }

    private void Start()
    {
        if (sakeroOptions == null || sakeroOptions.Length == 0)
            return;

        selectedIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(SelectedSakeroKey, 0),
            0,
            sakeroOptions.Length - 1
        );

        UpdateSelection();
    }

    private void SelectSakero(int index)
    {
        if (index < 0 || index >= sakeroOptions.Length)
            return;

        selectedIndex = index;

        PlayerPrefs.SetInt(SelectedSakeroKey, selectedIndex);
        PlayerPrefs.Save();

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        for (int i = 0; i < sakeroOptions.Length; i++)
        {
            if (sakeroOptions[i].selectionHighlight != null)
            {
                sakeroOptions[i].selectionHighlight.SetActive(
                    i == selectedIndex
                );
            }
        }
    }

    private void StartGame()
    {
        if (sakeroOptions == null || sakeroOptions.Length == 0)
            return;

        PlayerPrefs.SetInt(SelectedSakeroKey, selectedIndex);
        PlayerPrefs.Save();

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }
}
