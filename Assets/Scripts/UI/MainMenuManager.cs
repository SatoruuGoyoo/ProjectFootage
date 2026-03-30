using UnityEngine;
using UnityEngine.SceneManagement;

namespace SM.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        private void Start()
        {
            // Aseguramos que el panel principal esté visible al iniciar
            ShowMainPanel();
        }

        public void OnPlayClicked()
        {
            // Temporal: Carga la escena principal (índice 1). 
            // Escalabilidad: Aquí se puede llamar un panel de "Save Slots" en el futuro.
            SceneManager.LoadScene(1);
        }

        public void OnOptionsClicked()
        {
            mainPanel.SetActive(false);
            optionsPanel.SetActive(true);
        }

        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ShowMainPanel()
        {
            mainPanel.SetActive(true);
            optionsPanel.SetActive(false);
        }
    }
}
