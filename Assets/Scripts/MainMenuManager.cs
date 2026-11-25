
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
<<<<<<< Updated upstream
=======
using UnityEngine.SceneManagement;
using UnityEngine.UI;
>>>>>>> Stashed changes

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject sakeroSelectionPanel;

<<<<<<< Updated upstream
=======
    
    [Header("Continue (Save System)")]
    [Tooltip("The Continue button in the scene. Shown only when a save file exists.")]
    [SerializeField] private Button continueButton;

    private const string GameScene = "TowerBuilding";

>>>>>>> Stashed changes
    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        sakeroSelectionPanel.SetActive(false);

        ResetPointer();
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        sakeroSelectionPanel.SetActive(false);

        ResetPointer();
    }

    public void ShowSakeroSelection()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        sakeroSelectionPanel.SetActive(true);

        ResetPointer();
<<<<<<< Updated upstream
=======
    }
    private void Awake()
    {
        SaveSystem.ClearLoaded();
        //SetupContinueButton();
>>>>>>> Stashed changes
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ResetPointer()
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
