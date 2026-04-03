using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    private CharacterController characterController;
    private Vector3 currentWorldDir = Vector3.zero;
    private Vector2 lastRawInput = Vector2.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    public void Move(Vector2 input, Camera activeCamera)
    {
        if (config == null || activeCamera == null) return;

        Vector3 camForward = activeCamera.transform.forward;
        Vector3 camRight = activeCamera.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y = 0f; camRight.Normalize();

        bool hasInput = input.sqrMagnitude > 0.01f;
        bool inputChanged = Vector2.Distance(input, lastRawInput) > 0.15f;
        lastRawInput = input;

        if (!hasInput)
            currentWorldDir = Vector3.zero;
        else if (inputChanged)
            currentWorldDir = (camForward * input.y + camRight * input.x).normalized;

        if (currentWorldDir.sqrMagnitude < 0.01f) return;

        characterController.Move(currentWorldDir * config.MoveSpeed * Time.deltaTime);

        Quaternion targetRot = Quaternion.LookRotation(currentWorldDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, Time.deltaTime * config.TurnSpeed);
    }

    // Shooter-style: relativo al personaje, sin rotarlo (usado al grabar)
    public void MoveRelativeToSelf(Vector2 input)
    {
        if (config == null) return;
        Vector3 moveDir = transform.forward * input.y + transform.right * input.x;
        characterController.Move(moveDir * config.MoveSpeed * Time.deltaTime);
    }

    // Rota el cuerpo con el mouse (usado por la camcorder)
    public void Rotate(float rotateInput)
    {
        if (config == null) return;
        float turnAmount = rotateInput * config.TurnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }
}