using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    private CharacterController characterController;
    private Vector3 currentWorldDir = Vector3.zero;
    private Vector3 lastValidDir = Vector3.zero;
    private Vector2 lastRawInput = Vector2.zero;
    private float currentSpeedTank = 0f;
    private float currentSpeedModern = 0f;
    private float verticalVelocity = 0f;

    private void Awake() => characterController = GetComponent<CharacterController>();

    // --- Tank ---

    public void MoveTank(float moveInput)
    {
        if (config == null) return;

        float targetSpeed = moveInput * config.MoveSpeed;
        currentSpeedTank = Mathf.MoveTowards(currentSpeedTank, targetSpeed, config.Acceleration * Time.deltaTime);

        ApplyGravity();
        Vector3 movement = transform.forward * currentSpeedTank * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(movement);
    }

    public void Turn(float turnInput)
    {
        if (config == null) return;
        transform.Rotate(0, turnInput * config.TurnSpeed * Time.deltaTime, 0);
    }

    // --- Modern ---

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
        {
            currentWorldDir = (camForward * input.y + camRight * input.x).normalized;
            lastValidDir = currentWorldDir;
        }

        float targetSpeed = hasInput ? config.MoveSpeed : 0f;
        currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, config.Acceleration * Time.deltaTime);

        ApplyGravity();

        if (currentSpeedModern < 0.01f)
        {
            characterController.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
            return;
        }

        characterController.Move(lastValidDir * currentSpeedModern * Time.deltaTime
            + Vector3.up * verticalVelocity * Time.deltaTime);

        if (hasInput && currentSpeedModern > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lastValidDir),
                Time.deltaTime * config.RotationSmoothSpeed);
            FlattenRotation();
        }
    }

    public void MoveRelativeToSelf(Vector2 input)
    {
        if (config == null) return;

        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 flatRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        Vector3 inputDir = flatForward * input.y + flatRight * input.x;
        float targetSpeed = inputDir.sqrMagnitude > 0.01f ? config.MoveSpeed * config.RecordingSpeedMultiplier : 0f;
        currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, config.Acceleration * Time.deltaTime);

        ApplyGravity();
        Vector3 movement = inputDir.normalized * currentSpeedModern * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(movement);
    }

    // --- Shared ---

    // Direct yaw rotation in pre-calculated degrees. Used by CamcorderController for 1:1 sync.
    public void RotateDirect(float yawDegrees)
    {
        transform.Rotate(0, yawDegrees * config.RecordingRotationMultiplier, 0);
        FlattenRotation();
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded)
            verticalVelocity = -2f;
        else
            verticalVelocity += Physics.gravity.y * config.GravityMultiplier * Time.deltaTime;
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