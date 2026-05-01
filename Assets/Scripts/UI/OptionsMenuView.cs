using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuView : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject pausePanel;

    public void OnBackButton()
    {
        PlayerPrefs.Save();
        optionsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}