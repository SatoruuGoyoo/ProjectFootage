using UnityEngine;

public class CamcorderMotor : MonoBehaviour
{
    [Header("Setup")]
    public Transform camcorderPivot;
    public Transform camcorderCamera;

    [Header("Tilt Config")]
    public float tiltSpeed = 60f;
    public float tiltMinAngle = -30f;
    public float tiltMaxAngle = 30f;

    [Header("Rotate Config")]
    public float rotateSpeed = 80f;
    public float rotateMinAngle = -70f;
    public float rotateMaxAngle = 70f;

    public float LastRotateDelta { get; private set; }

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
        LastRotateDelta = rotateInput * rotateSpeed * Time.deltaTime;
        if (camcorderPivot == null) return;

        currentRotate += LastRotateDelta;
        currentRotate = Mathf.Clamp(currentRotate, rotateMinAngle, rotateMaxAngle);
        camcorderPivot.localEulerAngles = new Vector3(0f, currentRotate, 0f);
    }

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