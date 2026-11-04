using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject settings;

    public void PlayGame()
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
        settings.SetActive(!settings.activeSelf);
        StartCoroutine(ResetPointerState());
    }

    private IEnumerator ResetPointerState()
    {
        yield return null;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    
}
