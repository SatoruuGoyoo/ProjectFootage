using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;

    private CharacterController characterController;
    private float currentSpeedTank = 0f;
    private float currentSpeedModern = 0f;
    private float verticalVelocity = 0f;
    private Camera lastCamera = null;
    private Vector3 lockedWorldDir = Vector3.forward;
    private bool directionLocked = false;

    [SerializeField] float angleForInput = 75f;
    private void Awake() => characterController = GetComponent<CharacterController>();

    public bool HasInput { get; private set; }
    public bool IsMovingBackward { get; private set; }
    public bool IsSprinting { get; private set; }

    // Tank 

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

    // Modern 
    public void MoveRelativeToCamera(Vector2 input, Camera activeCamera, bool sprint)
    {

        if (config == null || activeCamera == null) return;

        Vector3 camForward = activeCamera.transform.forward;
        Vector3 camRight = activeCamera.transform.right;
        camForward.y = 0f; camForward.Normalize();
        camRight.y = 0f; camRight.Normalize();

        bool hasInput = input.sqrMagnitude > 0.01f;
        HasInput = hasInput;
        if (activeCamera != lastCamera)
        {
            lastCamera = activeCamera;
            lockedWorldDir = transform.forward;
            directionLocked = true;
        }

        if (hasInput)
        {
            Vector3 desiredDir = (camForward * input.y + camRight * input.x).normalized;

            if (directionLocked)
            {
                
                float angle = Vector3.Angle(lockedWorldDir, desiredDir);
                if (angle > angleForInput)
                    directionLocked = false;
            }

            if (!directionLocked)
                lockedWorldDir = desiredDir;
        }
        else
        {
            directionLocked = false;
        }

        IsSprinting = sprint && hasInput;
        float topSpeed = IsSprinting ? config.SprintSpeed : config.ModernMoveSpeed;
        float targetSpeed = hasInput ? topSpeed : 0f;
        float accel = targetSpeed > 0.01f ? config.Acceleration : config.Deceleration;
        currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, accel * Time.deltaTime);

        if (hasInput && currentSpeedModern > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lockedWorldDir),
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

    public void MoveRelativeToSelf(Vector2 input, bool tankSpeed = false)
    {
        if (config == null) return;

        Vector3 flatForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 flatRight = new Vector3(transform.right.x, 0f, transform.right.z).normalized;

        Vector3 inputDir = flatForward * input.y + flatRight * input.x; // strafe incluido
        IsMovingBackward = input.y < -0.01f;

        float speed = tankSpeed ? config.RecordingSpeedTank : config.RecordingSpeedModern;
        float targetSpeed = inputDir.sqrMagnitude > 0.01f ? speed : 0f;
        float accel = targetSpeed > 0.01f ? config.Acceleration : config.Deceleration;

        if (tankSpeed)
            currentSpeedTank = Mathf.MoveTowards(currentSpeedTank, targetSpeed, accel * Time.deltaTime);
        else
            currentSpeedModern = Mathf.MoveTowards(currentSpeedModern, targetSpeed, accel * Time.deltaTime);

        float finalSpeed = tankSpeed ? currentSpeedTank : currentSpeedModern;
        HasInput = inputDir.sqrMagnitude > 0.01f;

        ApplyGravity();
        Vector3 movement = inputDir.normalized * finalSpeed * Time.deltaTime;
        movement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(movement);
    }

    // Shared 

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

    private void FlattenRotation()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        transform.eulerAngles = euler;
    }
}