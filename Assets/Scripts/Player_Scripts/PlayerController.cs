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

        bool isTank = ControlSchemeManager.Instance.CurrentScheme
                      == ControlSchemeManager.Scheme.Tank;

        if (isTank)
            UpdateTank();
        else
            UpdateModern();
    }

    // ??? TANK ????????????????????????????????????????????????
    private void UpdateTank()
    {
        // Cámara arriba o grabando: mouse maneja todo, WASD bloqueado
        if (currentMode == PlayerMode.CameraMode) return;
        if (currentMode == PlayerMode.RecordingMode) return;

        // Exploration: movimiento tanque clásico
        motor.MoveTank(input.MoveForward);
        motor.Turn(input.Turn);
    }

    // ??? MODERN ??????????????????????????????????????????????
    private void UpdateModern()
    {
        // Exploration: movimiento relativo a cámara fija
        if (currentMode == PlayerMode.ExplorationMode)
        {
            Camera activeCam = CameraManager.Instance?.ActiveCamera;
            motor.MoveRelativeToCamera(input.MoveVector, activeCam);
        }

        // Cámara arriba o grabando: shooter-style (mouse rota vía CamcorderController)
        if (currentMode == PlayerMode.CameraMode ||
            currentMode == PlayerMode.RecordingMode)
        {
            motor.MoveRelativeToSelf(input.MoveVector);
        }
    }
}