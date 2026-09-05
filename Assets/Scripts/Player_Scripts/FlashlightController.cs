using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Light flashlight;
    [SerializeField] private Light indirectLight;

    [Header("Frustum Sync")]
    [Tooltip("Si está activo, la luz copia el range/angle del frustum de la cámara")]
    [SerializeField] private bool syncWithFrustum = true;
    [Tooltip("Cámara de referencia para sincronizar el rango. Si es null, se ignora.")]
    [SerializeField] private Camera referenceCamera;
    //[Tooltip("Multiplicador del rango de la luz vs el far clip plane")]
    //[SerializeField] private float rangeMultiplier = 1f;
    //[Tooltip("Padding extra al ángulo del spot vs el FOV de la cámara")]
    //[SerializeField] private float angleBonus = 5f;

    private bool _isCameraUp;

    private void Awake()
    {
        if (flashlight == null) flashlight = GetComponent<Light>();
        if (flashlight != null) flashlight.enabled = false;
        if (flashlight == null) indirectLight = GetComponent<Light>();
        if (flashlight != null) indirectLight.enabled = false;
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
        if (flashlight != null) flashlight.enabled = false;
        if (indirectLight != null) indirectLight.enabled = false;
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        _isCameraUp = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;
        if (flashlight != null) flashlight.enabled = _isCameraUp;
        if (indirectLight != null) indirectLight.enabled = _isCameraUp;
    }

    //private void LateUpdate()
    //{
    //    if (!syncWithFrustum || referenceCamera == null || flashlight == null || !_isCameraUp) return;

    //    flashlight.range = referenceCamera.farClipPlane * rangeMultiplier;
    //    flashlight.spotAngle = Mathf.Clamp(referenceCamera.fieldOfView + angleBonus, 1f, 179f);
    //}
}