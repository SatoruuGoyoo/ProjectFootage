using UnityEngine;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput input;
    private PlayerMotor motor;
    private PlayerView view;

    private void Awake()
    {
        input = GetComponent<PlayerInput>();
        motor = GetComponent<PlayerMotor>();
        view = GetComponent<PlayerView>();
    }

    private void Update()
    {
        motor.Move(input.Move);
        motor.Turn(input.Turn);

        // Update the view based on the current state (e.g., health, movement)
    }
}
