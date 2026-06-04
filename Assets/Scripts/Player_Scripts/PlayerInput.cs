using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    // Tank controls
    public float MoveForward { get; private set; }
    public float Turn { get; private set; }

    // Modern controls
    public Vector2 MoveVector { get; private set; }

    public bool IsSprinting { get; private set; }
    public bool Interact { get; private set; }
    public bool Decline { get; private set; }   // F — used by ConfirmationUI

    private PlayerInputActions actions;
    private InputAction _move;
    private InputAction _turn;
    private InputAction _sprint;
    private InputAction _interact;
    private InputAction _decline;

    private void Awake()
    {
        actions = new PlayerInputActions();
        _move = actions.Exploration.Move;
        _turn = actions.Exploration.Turn;
        _sprint = actions.Exploration.Sprint;
        _interact = actions.Exploration.Interact;
        _decline = actions.Exploration.Decline;   // add "Decline" → F in the Input Actions asset
    }

    private void OnEnable() => actions.Exploration.Enable();
    private void OnDisable() => actions.Exploration.Disable();

    private void Update()
    {
        MoveVector = _move.ReadValue<Vector2>();
        MoveForward = MoveVector.y;
        Turn = _turn.ReadValue<float>();
        IsSprinting = _sprint.ReadValue<float>() > 0.5f;
        Interact = _interact.WasPressedThisFrame();
        Decline = _decline.WasPressedThisFrame();
    }
}