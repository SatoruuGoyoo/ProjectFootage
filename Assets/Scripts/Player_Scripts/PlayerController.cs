using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    public static bool MovementBlocked = false;
    public static bool ForwardOnlyMode = false;

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
            motor.StopPlayer();
        else if (newMode != PlayerMode.ExplorationMode)
            motor.ClearTurn();
    }
    private void OnControllerSchemeChanged(ControlScheme newScheme) => currentScheme = newScheme;

    private void Update()
    {
        if (currentMode == PlayerMode.MenuCameraMode) return;
        if (currentMode == PlayerMode.InteractionMode) return;
        if (MovementBlocked) return;

        if (currentScheme == ControlScheme.Tank)
            UpdateTank();
        else
            UpdateModern();
    }

    private void UpdateTank()
    {
        float move = ForwardOnlyMode ? Mathf.Max(0f, input.MoveForward) : input.MoveForward;
        float turn = ForwardOnlyMode ? 0f : input.Turn;

        motor.MoveTank(move, input.IsSprinting);
        motor.Turn(currentMode == PlayerMode.ExplorationMode ? turn : 0f);
    }

    private void UpdateModern()
    {
        Camera activeCam = CameraManager.Instance?.ActiveCamera;

        Vector2 move = ForwardOnlyMode
            ? new Vector2(0f, Mathf.Max(0f, input.MoveVector.y))
            : input.MoveVector;

        if (currentMode == PlayerMode.ExplorationMode)
            motor.MoveRelativeToCamera(move, activeCam, input.IsSprinting);

        if (currentMode == PlayerMode.CameraMode)
            motor.MoveRelativeToSelf(ForwardOnlyMode ? new Vector2(0f, Mathf.Max(0f, input.MoveVector.y)) : input.MoveVector);

        if (currentMode == PlayerMode.RecordingMode)
            motor.MoveRelativeToSelf(new Vector2(0f, input.MoveForward));
    }
}