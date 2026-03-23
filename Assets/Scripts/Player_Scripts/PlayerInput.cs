using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public float Move { get; private set; }
    public float Turn { get; private set; }

    private PlayerInputActions actions;

    private void Awake()
    {
        actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        actions.Exploration.Enable();
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
    }

    private void Update()
    {
        Move = actions.Exploration.Move.ReadValue<float>();
        Turn = actions.Exploration.Turn.ReadValue<float>();
    }
}