using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    public static bool MovementBlocked = false;

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

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        currentMode = newMode;

        if (newMode == PlayerMode.InteractionMode || newMode == PlayerMode.MenuCameraMode)
        {
            motor.StopPlayer();
        }
    }

    private void OnControllerSchemeChanged(ControlScheme newScheme) => currentScheme = newScheme;

    private void Update()
    {
        if (currentMode == PlayerMode.MenuCameraMode) return;
        if (currentMode == PlayerMode.InteractionMode) return;
        if (MovementBlocked)
        {
            Debug.Log("[PlayerController] BLOCKED, ignorando movimiento");
            return;
     
        }
        if (currentScheme == ControlScheme.Tank)
            UpdateTank();
        else
            UpdateModern();
    }

    private void UpdateTank()
    {
        motor.MoveTank(input.MoveForward, input.IsSprinting);
        if (currentMode == PlayerMode.ExplorationMode)
            motor.Turn(input.Turn);
    }

    private void UpdateModern()
    {
        Camera activeCam = CameraManager.Instance?.ActiveCamera;

        if (currentMode == PlayerMode.ExplorationMode)
            motor.MoveRelativeToCamera(input.MoveVector, activeCam, input.IsSprinting);

        if (currentMode == PlayerMode.CameraMode)
            motor.MoveRelativeToSelf(input.MoveVector);

        if (currentMode == PlayerMode.RecordingMode)
            motor.MoveRelativeToSelf(new Vector2(0f, input.MoveForward));
    }
}