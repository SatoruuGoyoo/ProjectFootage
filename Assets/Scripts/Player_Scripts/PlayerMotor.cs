using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    public PlayerConfig config;
    private CharacterController characterController;

    // === Modern: state para suavizar el cambio de dirección ===
    private Vector3 currentWorldDir = Vector3.zero;
    private Vector2 lastRawInput = Vector2.zero;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    // ??? TANK ????????????????????????????????????????????????

    /// <summary>Mover adelante/atrás relativo al forward del personaje.</summary>
    public void MoveTank(float moveInput)
    {
        if (config == null) return;
        Vector3 movement = transform.forward * moveInput * config.MoveSpeed * Time.deltaTime;
        characterController.Move(movement);
    }

    /// <summary>Rotar al personaje con input directo (A/D). Usa config.TurnSpeed.</summary>
    public void Turn(float turnInput)
    {
        if (config == null) return;
        float turnAmount = turnInput * config.TurnSpeed * Time.deltaTime;
        transform.Rotate(0, turnAmount, 0);
    }

    // ??? MODERN ??????????????????????????????????????????????

    /// <summary>Movimiento relativo a la cámara activa (exploration/cameraMode).</summary>
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

        Quaternion targetRot = Quaternion.LookRotation(currentWorldDir);
        transform.rotation = Quaternion.Slerp(
    transform.rotation, targetRot, Time.deltaTime * config.RotationSmoothSpeed);

        // Nunca acumular pitch/roll del Slerp
        FlattenRotation();
    }

    /// <summary>Movimiento shooter-style relativo al personaje (usado al grabar en modern).</summary>
    public void MoveRelativeToSelf(Vector2 input)
    {
        if (config == null) return;

        // Forward/right limpios sin pitch — evita que W/S mueva en Y
        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        flatForward.Normalize();

        Vector3 flatRight = transform.right;
        flatRight.y = 0f;
        flatRight.Normalize();

        Vector3 moveDir = flatForward * input.y + flatRight * input.x;
        characterController.Move(moveDir * config.MoveSpeed * Time.deltaTime);
    }

    // ??? SHARED ??????????????????????????????????????????????

    /// <summary>
    /// Rotación directa en grados ya calculados. NO multiplica por velocidad ni deltaTime.
    /// Usado por CamcorderController para mantener sync 1:1 con CamcorderMotor.
    /// </summary>
    public void RotateDirect(float yawDegrees)
    {
        transform.Rotate(0, yawDegrees, 0);
        FlattenRotation();
    }

    /// <summary>Limpia cualquier pitch/roll residual. Solo Y sobrevive.</summary>
    private void FlattenRotation()
    {
        Vector3 euler = transform.eulerAngles;
        euler.x = 0f;
        euler.z = 0f;
        transform.eulerAngles = euler;
    }
}