using UnityEngine;

/// <summary>
/// Linterna que sigue la rotación de la camcorder camera y se
/// enciende/apaga al levantar/bajar la cámara (click derecho).
/// Ponelo en el GameObject que tenga el Light.
/// </summary>
public class FlashlightController : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("El mismo Transform que CamcorderMotor.camcorderCamera (el que tiltea)")]
    public Transform camcorderCamera;

    [Header("Offset")]
    [Tooltip("Offset de posición local respecto a la camcorder camera")]
    public Vector3 positionOffset = Vector3.zero;

    private Light flashlight;

    private void Awake()
    {
        flashlight = GetComponent<Light>();
        if (flashlight != null) flashlight.enabled = false;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        if (flashlight == null) return;

        
        bool cameraUp = newMode == PlayerMode.CameraMode
                     || newMode == PlayerMode.RecordingMode;

        flashlight.enabled = cameraUp;
    }

    private void LateUpdate()
    {
        if (camcorderCamera == null) return;
        transform.rotation = camcorderCamera.rotation;
        transform.position = camcorderCamera.position + camcorderCamera.TransformDirection(positionOffset);
    }
}