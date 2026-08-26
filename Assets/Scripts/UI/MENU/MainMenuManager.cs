using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace SM.UI
{
    public sealed class MainMenuManager : MonoBehaviour
    {
        [Header("Scene Transition")]
        [SerializeField] private SequenceRunner playSequence;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject optionsPanel;

        [Header("Navigation")]
        [SerializeField] private GameObject mainPanelFirstSelected;
        [SerializeField] private MenuKeyBoardNavigator keyboardNavigator;

        [Header("Unavailable Features")]
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button loadGameButton;

        [Header("Canvas")]
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private CanvasGroup menuGroup;
        [SerializeField] private bool startLocked = true;

        [Header("Sounds")]
        [SerializeField] private FMODUnity.EventReference clickEvent;
        [SerializeField] private FMODUnity.EventReference ambienceEvent;
        [SerializeField] private FMODUnity.EventReference unavailableEvent;

        private FMOD.Studio.EventInstance _ambience;
        private bool _menuLocked;
        private bool _interactable;

        public bool IsInteractable => _interactable && !_menuLocked;

        private void Awake()
        {
            _interactable = !startLocked;
            ApplyInteractable();
        }

        private void Start()
        {
            ShowMainPanel();

            if (!ambienceEvent.IsNull)
            {
                try
                {
                    _ambience = FMODUnity.RuntimeManager.CreateInstance(ambienceEvent);
                    _ambience.start();
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[MainMenuManager] No se pudo crear el evento de ambiente: {e.Message}");
                }
            }
        }

        private void OnDestroy()
        {
            _ambience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _ambience.release();
        }

        public void EnableInteraction()
        {
            if (_menuLocked || _interactable) return;

            _interactable = true;
            ApplyInteractable();
            FocusFirstSelected();
        }

        public void DisableInteraction()
        {
            if (!_interactable) return;

            _interactable = false;
            ApplyInteractable();
        }

        public void OnPlayClicked()
        {
            if (!IsInteractable) return;

            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            _ambience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            LockMenu();

            if (playSequence != null)
                playSequence.StartSequence();
        }

        public void OnOptionsClicked()
        {
            if (!IsInteractable) return;
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            ShowOptionsPanel();
        }

        public void OnLoadGameClicked()
        {
            if (!IsInteractable) return;
            ShowUnavailable(loadGameButton);
        }

        public void OnBackClicked()
        {
            if (!IsInteractable) return;
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            ShowMainPanel();
        }

        public void OnExitClicked()
        {
            if (!IsInteractable) return;
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            LockMenu();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void ShowUnavailable(Button button)
        {
            if (!unavailableEvent.IsNull)
                FMODUnity.RuntimeManager.PlayOneShot(unavailableEvent);

            if (button != null)
            {
                ButtonHoverEffect hover = button.GetComponent<ButtonHoverEffect>();
                hover?.FlashDenied();
            }
        }

        private void LockMenu()
        {
            _menuLocked = true;
            ApplyInteractable();

            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);
        }

        private void ShowMainPanel()
        {
            SetPanel(mainPanel, true);
            SetPanel(optionsPanel, false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            FocusFirstSelected();
        }

        private void ShowOptionsPanel()
        {
            SetPanel(mainPanel, false);
            SetPanel(optionsPanel, true);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void FocusFirstSelected()
        {
            if (keyboardNavigator != null) return;
            if (!IsInteractable || EventSystem.current == null) return;
            if (mainPanel == null || !mainPanel.activeInHierarchy) return;

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainPanelFirstSelected);
        }

        private void ApplyInteractable()
        {
            bool active = IsInteractable;

            if (menuGroup != null)
            {
                menuGroup.interactable = active;
                menuGroup.blocksRaycasts = active;
            }

            if (keyboardNavigator != null)
                keyboardNavigator.SetNavigationEnabled(active);

            if (!active && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private static void SetPanel(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mainPanel == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] mainPanel is not assigned.", this);
            if (optionsPanel == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] optionsPanel is not assigned.", this);
            if (menuCanvas == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] menuCanvas is not assigned.", this);
            if (menuGroup == null)
                Debug.LogWarning($"[{nameof(MainMenuManager)}] menuGroup is not assigned — la interacción no se va a poder bloquear durante la intro.", this);
        }
#endif
    }
}