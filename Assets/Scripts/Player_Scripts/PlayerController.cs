using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerMotor motor;
    private PlayerView view;

    private PlayerMode currentMode = PlayerMode.ExplorationMode;
    private ControlScheme currentScheme = ControlScheme.Tank;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        motor = GetComponent<PlayerMotor>();
        view = GetComponent<PlayerView>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
        GameEvents.OnControllerSchemeChanged += OnControllerSchemeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
        GameEvents.OnControllerSchemeChanged -= OnControllerSchemeChanged;
    }

    private void OnPlayerModeChanged(PlayerMode newMode) => currentMode = newMode;
    private void OnControllerSchemeChanged(ControlScheme newScheme) => currentScheme = newScheme;

    private void Update()
    {
        if (currentMode == PlayerMode.MenuCameraMode) return;

        if (currentScheme == ControlScheme.Tank)
            UpdateTank();
        else
            UpdateModern();
    }

    private void UpdateTank()
    {
        if (currentMode == PlayerMode.CameraMode) return;
        if (currentMode == PlayerMode.RecordingMode) return;

        motor.MoveTank(input.MoveForward);
        motor.Turn(input.Turn);
    }

    private void UpdateModern()
    {
        if (currentMode == PlayerMode.ExplorationMode)
        {
            Camera activeCam = CameraManager.Instance?.ActiveCamera;
            motor.MoveRelativeToCamera(input.MoveVector, activeCam);
        }

        if (currentMode == PlayerMode.CameraMode ||
            currentMode == PlayerMode.RecordingMode)
        {
            motor.MoveRelativeToSelf(input.MoveVector);
        }
    }
}