using UnityEngine;

public class CamcorderMotor : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Transform camcorderPivot;
    [SerializeField] private Transform camcorderCamera;

    [Header("Tilt Config")]
    [SerializeField] private float tiltSpeed = 60f;
    [SerializeField] private float tiltMinAngle = -30f;
    [SerializeField] private float tiltMaxAngle = 30f;

    [Header("Rotate Config")]
    [SerializeField] private float rotateSpeed = 80f;
    [SerializeField] private float rotateMinAngle = -70f;
    [SerializeField] private float rotateMaxAngle = 70f;

    public float LastRotateDelta { get; private set; }
    public float RotateSpeed => rotateSpeed;
    public float CurrentTilt => currentTilt;

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