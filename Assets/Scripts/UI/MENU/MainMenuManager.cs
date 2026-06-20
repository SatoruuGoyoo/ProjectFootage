using UnityEngine;

namespace SM.UI
{
    public sealed class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Transition")]
        [SerializeField] private string gameSceneName = "SCN_Integration";

        [Header("Intro Text")]
        [TextArea(2, 5)]
        [SerializeField] private string introText = "Sk4rz_26\n2:21 AM";

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Interaction")]
        [SerializeField] private CanvasGroup menuCanvasGroup;

        private bool _menuLocked;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ShowMainPanel();
        }

        public void OnPlayClicked()
        {
            if (_menuLocked || FadeManager.Instance.IsBusy)
                return;

            LockMenu();

            FadeManager.Instance.FadeToScene(
                gameSceneName,
                introText
            );
        }

        public void OnOptionsClicked()
        {
            if (_menuLocked) return;

            SetPanel(mainPanel, false);
            SetPanel(optionsPanel, true);
        }

        public void OnBackClicked()
        {
            if (_menuLocked) return;

            ShowMainPanel();
        }

        public void OnExitClicked()
        {
            if (_menuLocked) return;

            LockMenu();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void LockMenu()
        {
            _menuLocked = true;

            if (menuCanvasGroup == null)
                return;

            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        private void ShowMainPanel()
        {
            SetPanel(mainPanel, true);
            SetPanel(optionsPanel, false);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(gameSceneName))
                Debug.LogWarning($"[{nameof(MainMenuManager)}] gameSceneName is empty.", this);

            if (mainPanel == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] mainPanel is not assigned.", this);

            if (optionsPanel == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] optionsPanel is not assigned.", this);
        }
#endif
    }
}