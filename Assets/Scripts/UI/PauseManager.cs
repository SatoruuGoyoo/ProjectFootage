using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    private PlayerInputActions actions;
    private bool isPaused = false;

    private void Awake() => actions = new PlayerInputActions();

    private void OnEnable()
    {
        actions.UI.Enable();
        actions.UI.Pause.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        actions.UI.Pause.performed -= OnPausePressed;
        actions.UI.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext ctx)
    {
        if (isPaused) Resume(); else Pause();
    }

    private void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        GameEvents.PauseChanged(true);      
        actions.Exploration.Disable();
        actions.MenuCamera.Disable();
        actions.Camera.Disable();
    }

    private void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        GameEvents.PauseChanged(false);
        actions.Exploration.Enable();
        actions.MenuCamera.Enable();
        actions.Camera.Enable();

    }
}