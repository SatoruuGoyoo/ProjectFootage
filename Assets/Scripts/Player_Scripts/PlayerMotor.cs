using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    private CharacterController characterController;
    private Vector3 currentWorldDir = Vector3.zero;
    private Vector2 lastRawInput = Vector2.zero;

    private void Awake() => characterController = GetComponent<CharacterController>();

    // --- Tank ---

    // Moves forward/back relative to the character's facing direction
    public void MoveTank(float moveInput)
    {
        if (config == null) return;
        characterController.Move(transform.forward * moveInput * config.MoveSpeed * Time.deltaTime);
    }

    // Rotates the character directly using A/D input
    public void Turn(float turnInput)
    {
        if (config == null) return;
        transform.Rotate(0, turnInput * config.TurnSpeed * Time.deltaTime, 0);
    }

    // --- Modern ---

    // Moves relative to the active fixed camera's orientation
    public void MoveRelativeToCamera(Vector2 input, Camera activeCamera)
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

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(currentWorldDir),
            Time.deltaTime * config.RotationSmoothSpeed);

        FlattenRotation();
    }

    // Moves relative to the character's own facing direction (used in recording mode)
    public void MoveRelativeToSelf(Vector2 input)
    {
        if (config == null) return;

        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 flatRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        characterController.Move((flatForward * input.y + flatRight * input.x) * config.MoveSpeed * Time.deltaTime);
    }

    // --- Shared ---

    // Direct yaw rotation in pre-calculated degrees. Used by CamcorderController for 1:1 sync.
    public void RotateDirect(float yawDegrees)
    {
        transform.Rotate(0, yawDegrees, 0);
        FlattenRotation();
    }

    // Strips any accumulated pitch/roll — only Y rotation survives
    private void FlattenRotation()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        transform.eulerAngles = euler;
    }
}