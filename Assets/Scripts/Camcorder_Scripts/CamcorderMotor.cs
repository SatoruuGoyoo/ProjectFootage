using UnityEngine;

public class CamcorderMotor : MonoBehaviour
{
    [Header("Setup")]
    public Transform camcorderCamera;

    [Header("Config")]
    public float tiltSpeed = 60f;
    public float tiltMinAngle = -30f;
    public float tiltMaxAngle = 30f;

    private float currentTilt = 0f;

    public void Tilt(float tiltInput)
    {
        currentTilt -= tiltInput * tiltSpeed * Time.deltaTime;
        currentTilt = Mathf.Clamp(currentTilt, tiltMinAngle, tiltMaxAngle);
        camcorderCamera.localEulerAngles = new Vector3(currentTilt, 0f, 0f);
    }
}