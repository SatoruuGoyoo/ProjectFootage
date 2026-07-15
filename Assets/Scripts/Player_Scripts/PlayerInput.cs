using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static bool SprintBlocked = false;
    public static PlayerInputActions Actions { get; private set; }

    public float MoveForward { get; private set; }
    public float Turn { get; private set; }
    public Vector2 MoveVector { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool Interact { get; private set; }
    public bool Decline { get; private set; }

    private PlayerInputActions actions;
    private InputAction _move;
    private InputAction _turn;
    private InputAction _sprint;
    private InputAction _interact;
    private InputAction _decline;

    private PlayerMode _currentMode = PlayerMode.ExplorationMode;

    private void Awake()
    {
        actions = new PlayerInputActions();
        Actions = actions;
        _move = actions.Exploration.Move;
        _turn = actions.Exploration.Turn;
        _sprint = actions.Exploration.Sprint;
        _interact = actions.Exploration.Interact;
        _decline = actions.Exploration.Decline;
    }

    private void OnEnable()
    {
        actions.Exploration.Enable();
        actions.UI.Enable();
        GameEvents.OnPlayerModeChanged += OnModeChanged;
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
        actions.UI.Disable();
        GameEvents.OnPlayerModeChanged -= OnModeChanged;
    }

    private void OnModeChanged(PlayerMode newMode)
    {
        _currentMode = newMode;
    }

    private void Update()
    {
        Interact = _interact.WasPressedThisFrame();
        Decline = _decline.WasPressedThisFrame();

        if (_currentMode == PlayerMode.InteractionMode || _currentMode == PlayerMode.MenuCameraMode)
        {
            MoveVector = Vector2.zero;
            MoveForward = 0f;
            Turn = 0f;
            IsSprinting = false;
            return;
        }

        MoveVector = _move.ReadValue<Vector2>();
        MoveForward = MoveVector.y;
        Turn = _turn.ReadValue<float>();
        IsSprinting = _sprint.ReadValue<float>() > 0.5f && !SprintBlocked;
    }
}