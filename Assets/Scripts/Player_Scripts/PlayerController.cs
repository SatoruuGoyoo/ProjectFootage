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
        // En tank, Recording bloquea todo el movimiento del player
        if (currentMode == PlayerMode.RecordingMode) return;

        if (currentMode == PlayerMode.ExplorationMode)
            motor.MoveTank(input.MoveForward);

        motor.Turn(input.Turn);
    }

    // ??? MODERN ??????????????????????????????????????????????
    private void UpdateModern()
    {
        // Exploration o CameraMode: movimiento relativo a cámara
        if (currentMode == PlayerMode.ExplorationMode ||
            currentMode == PlayerMode.CameraMode)
        {
            Camera activeCam = CameraManager.Instance?.ActiveCamera;
            motor.MoveRelativeToCamera(input.MoveVector, activeCam);
        }

        // Recording: shooter-style relativo a sí mismo (mouse rota vía CamcorderController)
        if (currentMode == PlayerMode.RecordingMode)
            motor.MoveRelativeToSelf(input.MoveVector);
    }
}