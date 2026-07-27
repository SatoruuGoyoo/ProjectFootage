using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private PauseMenuUI ui;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool _isPaused;
    private bool _isTransitioning;

    public bool IsPaused => _isPaused;

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
            Toggle();
    }

    private void Toggle()
    {
        if (_isTransitioning) return;

        if (_isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (_isPaused) return;

        _isPaused = true;
        Time.timeScale = 0f;

        if (MouseCursorController.Instance != null)
            MouseCursorController.Instance.RequestCursor();

        GameEvents.PauseChanged(true);
        ui.Show();
    }

    public void Resume()
    {
        if (!_isPaused) return;

        _isTransitioning = true;
        ui.Hide(OnHideFinished);
    }

    private void OnHideFinished()
    {
        _isPaused = false;
        _isTransitioning = false;
        Time.timeScale = 1f;

        if (MouseCursorController.Instance != null)
            MouseCursorController.Instance.ReleaseCursor();

        GameEvents.PauseChanged(false);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;

        if (MouseCursorController.Instance != null)
            MouseCursorController.Instance.ReleaseCursor();

        GameEvents.PauseChanged(false);
        ui.LoadMainMenu();
    }
}