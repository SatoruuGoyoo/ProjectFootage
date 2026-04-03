using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerMotor motor;
    private PlayerView view;

    private PlayerMode currentMode = PlayerMode.ExplorationMode;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        motor = GetComponent<PlayerMotor>();
        view = GetComponent<PlayerView>();
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;

    private void OnPlayerModeChanged(PlayerMode newMode) => currentMode = newMode;

    private void Update()
    {
        if (currentMode == PlayerMode.MenuCameraMode) return;

        // Camara arriba pero sin grabar: movimiento libre normal
        if (currentMode == PlayerMode.ExplorationMode ||
            currentMode == PlayerMode.CameraMode)
            motor.Move(input.Move, CameraManager.Instance.ActiveCamera);

        // Grabando: shooter-style, solo el mouse rota
        if (currentMode == PlayerMode.RecordingMode)
            motor.MoveRelativeToSelf(input.Move);
    }
}