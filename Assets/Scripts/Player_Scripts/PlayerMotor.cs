using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Move(float moveInput)
    {
        if (config == null) return;

        Vector3 movement = transform.forward * moveInput * config.MoveSpeed * Time.deltaTime;
        characterController.Move(movement);
    }

    public void Turn(float turnInput)
    {
        if (config == null) return;

        float turnAmount = turnInput * config.TurnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }
}