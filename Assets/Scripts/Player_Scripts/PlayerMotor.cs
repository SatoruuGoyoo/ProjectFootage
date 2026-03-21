using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    public void Move(float moveInput)
    {
        if (config == null) return;

        // Movement
        Vector3 movement = transform.forward * moveInput * config.MoveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    public void Turn(float turnInput)
    {
        if (config == null) return;

        // Rotation
        float turnAmount = turnInput * config.TurnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }
}
