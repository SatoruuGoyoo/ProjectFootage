using UnityEngine;

public class CamcorderMotor : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Gira en Y (horizontal) — ambos schemes")]
    public Transform camcorderPivot;
    [Tooltip("Gira en X (tilt) — usado en ambos schemes")]
    public Transform camcorderCamera;

    [Header("Tilt Config")]
    public float tiltSpeed = 60f;
    public float tiltMinAngle = -30f;
    public float tiltMaxAngle = 30f;

    [Header("Rotate Config")]
    public float rotateSpeed = 80f;
    public float rotateMinAngle = -70f;
    public float rotateMaxAngle = 70f;

    private float currentTilt = 0f;
    private float currentRotate = 0f;

    /// <summary>El delta sin clamp del último Rotate. Usado para sincronizar al player body.</summary>
    public float LastRotateDelta { get; private set; }

    /// <summary>Tilt vertical de la cámara (ambos schemes).</summary>
    public void Tilt(float tiltInput)
    {
        currentTilt -= tiltInput * tiltSpeed * Time.deltaTime;
        currentTilt = Mathf.Clamp(currentTilt, tiltMinAngle, tiltMaxAngle);
        camcorderCamera.localEulerAngles = new Vector3(currentTilt, 0f, 0f);
    }

    /// <summary>Rotación horizontal del pivot. Expone LastRotateDelta para sync con player.</summary>
    public void Rotate(float rotateInput)
    {
        LastRotateDelta = rotateInput * rotateSpeed * Time.deltaTime;

        if (camcorderPivot == null) return;

        currentRotate += LastRotateDelta;
        currentRotate = Mathf.Clamp(currentRotate, rotateMinAngle, rotateMaxAngle);
        camcorderPivot.localEulerAngles = new Vector3(0f, currentRotate, 0f);
    }

    /// <summary>Reset completo (tilt + rotate si aplica).</summary>
    public void ResetRotation()
    {
        currentTilt = 0f;
        currentRotate = 0f;

        if (camcorderCamera != null)
            camcorderCamera.localEulerAngles = Vector3.zero;

        if (camcorderPivot != null)
            camcorderPivot.localEulerAngles = Vector3.zero;
    }
}