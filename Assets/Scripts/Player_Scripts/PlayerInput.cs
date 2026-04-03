using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lee todos los inputs y los expone en ambos formatos.
/// IMPORTANTE: En el InputActions asset, configurá "Move" como Vector2 (composite WASD)
/// y mantené "Turn" como 1D axis separado. 
/// Tank usa MoveForward (.y) + Turn, Modern usa MoveVector completo.
/// </summary>
public class PlayerInput : MonoBehaviour
{
    // === Tank format ===
    public float MoveForward { get; private set; }
    public float Turn { get; private set; }

    // === Modern format ===
    public Vector2 MoveVector { get; private set; }

    private PlayerInputActions actions;

    private void Awake()
    {
        actions = new PlayerInputActions();
    }

    private void OnEnable() => actions.Exploration.Enable();
    private void OnDisable() => actions.Exploration.Disable();

    private void Update()
    {
        // Leer Vector2 completo (funciona si Move es composite 2D)
        MoveVector = actions.Exploration.Move.ReadValue<Vector2>();

        // Tank: forward/back del eje Y del vector
        MoveForward = MoveVector.y;

        // Turn: acción separada para tank (A/D o flechas)
        Turn = actions.Exploration.Turn.ReadValue<float>();
    }
}