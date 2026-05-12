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

    private PlayerInputActions actions;

    private void Awake() => actions = new PlayerInputActions();
    private void OnEnable() => actions.Exploration.Enable();
    private void OnDisable() => actions.Exploration.Disable();


    private void Update()
    {
        MoveVector = actions.Exploration.Move.ReadValue<Vector2>();
        MoveForward = MoveVector.y;
        Turn = actions.Exploration.Turn.ReadValue<float>();
        IsSprinting = actions.Exploration.Sprint.ReadValue<float>() > 0.5f; 
    }
}