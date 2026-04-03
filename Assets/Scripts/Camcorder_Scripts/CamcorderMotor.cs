using UnityEngine;

public class CamcorderMotor : MonoBehaviour
{
    [Header("Setup")]
    public Transform camcorderPivot;   // Gira en Y (horizontal) — nuevo
    public Transform camcorderCamera;  // Gira en X (tilt)       — el que ya tenías

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

    public void Tilt(float tiltInput)
    {
        currentTilt -= tiltInput * tiltSpeed * Time.deltaTime;
        currentTilt = Mathf.Clamp(currentTilt, tiltMinAngle, tiltMaxAngle);
        camcorderCamera.localEulerAngles = new Vector3(currentTilt, 0f, 0f);
    }

    public void Rotate(float rotateInput)
    {
        currentRotate += rotateInput * rotateSpeed * Time.deltaTime;
        currentRotate = Mathf.Clamp(currentRotate, rotateMinAngle, rotateMaxAngle);
        camcorderPivot.localEulerAngles = new Vector3(0f, currentRotate, 0f);
    }

    public void ResetRotation()
    {
        currentRotate = 0f;
        currentTilt = 0f;
        camcorderPivot.localEulerAngles = Vector3.zero;
        camcorderCamera.localEulerAngles = Vector3.zero;
    }
}