
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject sakeroSelectionPanel;

    private void Start()
    {
        ShowMainMenu();
    }

    [Header("Continue (Save System)")]
    [Tooltip("The Continue button in the scene. Shown only when a save file exists.")]
    [SerializeField] private Button continueButton;

    private const string GameScene = "TowerBuilding";

    private void Awake()
    {
        SaveSystem.ClearLoaded();
        SetupContinueButton();
    }

    public void ShowMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("TowerBuilding");
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    
    public void ToggleSettings()
    {
        StartCoroutine(ResetPointerState());
    }

    private IEnumerator ResetPointerState()
    {
        yield return null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}