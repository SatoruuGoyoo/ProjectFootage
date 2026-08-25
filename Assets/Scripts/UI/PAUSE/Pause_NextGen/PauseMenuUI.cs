using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] private MenuKeyBoardNavigator keyboardNavigator;

    [Header("Controller")]
    [SerializeField] private PauseController controller;

    [Header("Animator")]
    [SerializeField] private PauseAnimator menuAnimator;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("Scene")]
    [SerializeField] private SceneField mainMenuScene;

    [Header("Timing")]
    [SerializeField] private float fadeOutDuration = 0.8f;
    [SerializeField] private float fadeInDuration = 0.8f;

    [Header("Sounds")]
    [SerializeField] private string clickEvent = "event:/MainMenu/UI - UX/UI - ButtonClick";
    [SerializeField] private string unavailableEvent;

    private System.Action _onHideFinished;
    private bool _listenersRegistered;

    private void RegisterListeners()
    {
        if (_listenersRegistered) return;
        _listenersRegistered = true;

        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (saveButton != null) saveButton.onClick.AddListener(OnSaveClicked);
        if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnEnable()
    {
        if (menuAnimator != null)
            menuAnimator.OnCloseAnimationFinished += HandleCloseFinished;
    }

    private void OnDisable()
    {
        if (menuAnimator != null)
            menuAnimator.OnCloseAnimationFinished -= HandleCloseFinished;
    }

    public void Show()
    {
        RegisterListeners();
        gameObject.SetActive(true);
        if (menuAnimator != null) menuAnimator.PlayOpen();
        if (keyboardNavigator != null)
        {
            keyboardNavigator.SetNavigationEnabled(true);
            
        }
    }

    public void Hide(System.Action onFinished)
    {
        _onHideFinished = onFinished;

        if (keyboardNavigator != null)
            keyboardNavigator.SetNavigationEnabled(false);

        if (menuAnimator != null)
        {
            menuAnimator.PlayClose();
        }
        else
        {
            gameObject.SetActive(false);
            _onHideFinished?.Invoke();
            _onHideFinished = null;
        }
    }

    private void HandleCloseFinished()
    {
        gameObject.SetActive(false);
        _onHideFinished?.Invoke();
        _onHideFinished = null;
    }

    public void LoadMainMenu()
    {
        StartCoroutine(LoadMainMenuRoutine());
    }

    private IEnumerator LoadMainMenuRoutine()
    {
        if (FadeManager.Instance != null)
        {
            yield return FadeManager.Instance.FadeOut(fadeOutDuration);

            FadeManager.Instance.RequestFadeInOnNextLoad(fadeInDuration);

            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuScene);
        }
    }

    private void OnResumeClicked()
    {
        PlayClick();
        controller.Resume();
    }

    private void OnSaveClicked()
    {
        ShowUnavailable(saveButton);
    }

    private void OnOptionsClicked()
    {
        ShowUnavailable(optionsButton);
    }

    private void OnQuitClicked()
    {
        PlayClick();
        controller.QuitToMainMenu();
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

    private void PlayClick()
    {
        if (!string.IsNullOrEmpty(clickEvent))
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
    }
}