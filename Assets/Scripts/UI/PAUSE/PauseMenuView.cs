using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenuView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;

    [Header("Default Selection")]
    [SerializeField] private GameObject firstSelected;

    private void OnEnable() => GameEvents.OnPauseChanged += OnPauseChanged;
    private void OnDisable() => GameEvents.OnPauseChanged -= OnPauseChanged;

    private void OnPauseChanged(bool paused)
    {
        pausePanel.SetActive(paused);
        if (!paused)
        {
            optionsPanel.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null); 
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(firstSelected); 
        }

        if (!paused) optionsPanel.SetActive(false);
    }

    public void OnResumeButton()
    {
        GameEvents.PauseChanged(false);
    }

    public void OnOptionsButton()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("SCN_MainMenu"); 
    }

    public void OnQuitButton()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}