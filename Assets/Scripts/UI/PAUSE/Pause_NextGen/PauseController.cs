using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private PauseMenuUI menuUI;

    private bool _isPaused;
    private bool _isClosing;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (InputLock.AllBlocked) return;

        if (_isPaused) Resume();
        else Pause();
    }

    private void Pause()
    {
        if (_isPaused) return;

        _isPaused = true;
        _isClosing = false;
        Time.timeScale = 0f;
        GameEvents.PauseChanged(true);

        if (menuUI != null) menuUI.Show();
    }

    public void Resume()
    {
        if (!_isPaused || _isClosing) return;

        _isClosing = true;
        Time.timeScale = 1f;
        GameEvents.PauseChanged(false);

        if (menuUI != null)
            menuUI.Hide(OnHideFinished);
        else
            OnHideFinished();
    }

    private void OnHideFinished()
    {
        _isPaused = false;
        _isClosing = false;
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        _isPaused = false;
        _isClosing = false;

        ResetStaticState();

        if (menuUI != null)
            menuUI.LoadMainMenu();
    }

    private void ResetStaticState()
    {
        InputLock.AllBlocked = false;
        PlayerController.MovementBlocked = false;
        PlayerController.ForwardOnlyMode = false;
        PlayerInput.SprintBlocked = false;
        CamcorderController.LiftInputBlocked = false;
        CamcorderController.RecordInputBlocked = false;
        CamcorderMenuController.MenuInputBlocked = false;
    }
}