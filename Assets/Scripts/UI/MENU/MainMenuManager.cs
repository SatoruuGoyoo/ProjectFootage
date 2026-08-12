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

        [Header("Unavailable Features")]
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button loadGameButton;

        [Header("Canvas")]
        [SerializeField] private Canvas menuCanvas;

        [Header("Sounds")]
        [SerializeField] private string clickEvent = "event:/MainMenu/UI - UX/UI - ButtonClick";
        [SerializeField] private string ambienceEvent = "event:/MainMenu/Ambient/MenuMusic";
        [SerializeField] private string unavailableEvent;

        private FMOD.Studio.EventInstance _ambience;
        private bool _menuLocked;

        private void Start()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            ShowMainPanel();

            if (!string.IsNullOrEmpty(ambienceEvent))
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

        public void OnPlayClicked()
        {
            if (_menuLocked) return;

            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            _ambience.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            LockMenu();

            if (playSequence != null)
                playSequence.StartSequence();
        }

        public void OnOptionsClicked()
        {
            if (_menuLocked) return;
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            ShowOptionsPanel();
        }

        public void OnLoadGameClicked()
        {
            if (_menuLocked) return;
            ShowUnavailable(loadGameButton);
        }

        public void OnBackClicked()
        {
            if (_menuLocked) return;
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
            ShowMainPanel();
        }

        public void OnExitClicked()
        {
            if (_menuLocked) return;
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
            if (!string.IsNullOrEmpty(unavailableEvent))
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
            if (menuCanvas != null)
                menuCanvas.gameObject.SetActive(false);
        }

        private void ShowMainPanel()
        {
            SetPanel(mainPanel, true);
            SetPanel(optionsPanel, false);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;


            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(mainPanelFirstSelected);
        }

        private void ShowOptionsPanel()
        {
            SetPanel(mainPanel, false);
            SetPanel(optionsPanel, true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;


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
        }
#endif
    }
}