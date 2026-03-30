using UnityEngine;
using UnityEngine.SceneManagement;

namespace SM.UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private string gameSceneName = "GameScene";
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        private void Start()
        {
            ShowMainPanel();
        }

        public void OnPlayClicked()
        {
            SceneManager.LoadScene(gameSceneName);
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
