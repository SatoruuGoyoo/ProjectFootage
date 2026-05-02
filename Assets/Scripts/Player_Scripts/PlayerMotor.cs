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

    public bool HasInput { get; private set; }
    public bool IsMovingBackward { get; private set; }
    public bool IsSprinting { get; private set; }

    // --- Tank ---

    public void MoveTank(float moveInput, bool sprint)
    {
        if (config == null) return;

        IsMovingBackward = moveInput < -0.01f;
        IsSprinting = sprint && moveInput > 0.01f; 
        float topSpeed = IsSprinting ? config.SprintSpeed : config.MoveSpeed;
        float targetSpeed = moveInput * topSpeed;
        float accel = Mathf.Abs(targetSpeed) > 0.01f ? config.Acceleration : config.Deceleration;
        currentSpeedTank = Mathf.MoveTowards(currentSpeedTank, targetSpeed, accel * Time.deltaTime);
        HasInput = Mathf.Abs(moveInput) > 0.01f;

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

    public void MoveRelativeToCamera(Vector2 input, Camera activeCamera, bool sprint)
    {
        if (config == null || activeCamera == null) return;

        Vector3 camForward = activeCamera.transform.forward;
        Vector3 camRight = activeCamera.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y = 0f; camRight.Normalize();

        bool hasInput = input.sqrMagnitude > 0.01f;
        HasInput = hasInput;
        IsSprinting = sprint && hasInput;
        float topSpeed = IsSprinting ? config.SprintSpeed : config.ModernMoveSpeed;

        float targetSpeed = hasInput ? topSpeed : 0f;
        float accel = targetSpeed > 0.01f ? config.Acceleration : config.Deceleration;
        currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, accel * Time.deltaTime);

      
        if (hasInput && currentSpeedModern > 0.1f)
        {
            Vector3 desiredDir = (camForward * input.y + camRight * input.x).normalized;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(desiredDir),
                Time.deltaTime * config.RotationSmoothSpeed);
            FlattenRotation();
        }

        ApplyGravity();

        
        if (currentSpeedModern < 0.01f)
        {
            characterController.Move(new Vector3(0f, verticalVelocity * Time.deltaTime, 0f));
            return;
        }

        characterController.Move(transform.forward * currentSpeedModern * Time.deltaTime
            + Vector3.up * verticalVelocity * Time.deltaTime);
    }

    public void MoveRelativeToSelf(float forwardInput, bool tankSpeed = false)
    {
        if (config == null) return;

        IsMovingBackward = forwardInput < -0.01f; 

        float speed = tankSpeed ? config.RecordingSpeedTank : config.RecordingSpeedModern;
        float targetSpeed = Mathf.Abs(forwardInput) > 0.01f ? forwardInput * speed : 0f;
        float accel = Mathf.Abs(targetSpeed) > 0.01f ? config.Acceleration : config.Deceleration;

        if (tankSpeed)
            currentSpeedTank = Mathf.MoveTowards(currentSpeedTank, targetSpeed, accel * Time.deltaTime);
        else
            currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, accel * Time.deltaTime);

        float finalSpeed = tankSpeed ? currentSpeedTank : currentSpeedModern;
        HasInput = Mathf.Abs(forwardInput) > 0.01f;

        ApplyGravity();
        Vector3 movement = transform.forward * finalSpeed * Time.deltaTime;
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