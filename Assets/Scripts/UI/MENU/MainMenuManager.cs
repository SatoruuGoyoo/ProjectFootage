using UnityEngine;

namespace SM.UI
{
    /// <summary>
    /// Controls main menu panel navigation and delegates scene transitions
    /// to FadeManager. Does not know about scene loading internals.
    /// </summary>
    public sealed class MainMenuManager : MonoBehaviour
    {
        // ── Inspector Config ──────────────────────────────────────────────
        [Header("Scene Transition")]
        [SerializeField] private string gameSceneName = "SCN_Integration";

        [Header("Intro Text")]
        [TextArea(2, 5)]
        [SerializeField] private string introText = "Sk4rz_26\n2:21 AM";

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        // ── Lifecycle ─────────────────────────────────────────────────────

        private void Start()
        {
            ShowMainPanel();
        }

        // ── Button Handlers ───────────────────────────────────────────────

        public void OnPlayClicked()
        {
            if (FadeManager.Instance.IsBusy) return;
            FadeManager.Instance.FadeToScene(gameSceneName, introText);
        }

        public void OnOptionsClicked()
        {
            SetPanel(mainPanel, false);
            SetPanel(optionsPanel, true);
        }

        public void OnBackClicked()
        {
            ShowMainPanel();
        }

        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Panel Helpers ─────────────────────────────────────────────────

        private void ShowMainPanel()
        {
            SetPanel(mainPanel, true);
            SetPanel(optionsPanel, false);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
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