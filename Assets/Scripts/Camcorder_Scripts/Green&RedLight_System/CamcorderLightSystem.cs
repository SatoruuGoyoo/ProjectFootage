using System.Collections.Generic;
using UnityEngine;

public class CamcorderLightSystem : MonoBehaviour
{
    public static CamcorderLightSystem Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Reference to the camcorder camera")]
    [SerializeField] private Camera camcorderCamera;

    [Header("Config")]
    [Tooltip("How close the target needs to be in the green zone to trigger an event")]
    [SerializeField] private float centerThreshold = 0.1f;
    [Tooltip("Maximum distance in meters to detect a target")]
    [SerializeField] private float maxDetectionDistance = 10f;

    private readonly List<ICamcorderTarget> targets = new();
    private readonly HashSet<ICamcorderTarget> _centeredTargets = new();
    private bool currentState = false;
    private bool isCameraUp = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        isCameraUp = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;

        if (!isCameraUp)
        {
            ClearAllCentered();
            SetState(false);
        }
    }

    private void Start() => RefreshTargets();

    private void Update()
    {
        if (!isCameraUp) return;
        CheckTargets();
    }

    public void Register(ICamcorderTarget target)
    {
        if (!targets.Contains(target)) targets.Add(target);
    }

    public void Unregister(ICamcorderTarget target)
    {
        if (_centeredTargets.Remove(target))
            (target as ICenteredAware)?.OnCenteredChanged(false);
        targets.Remove(target);
    }

    public void RefreshTargets()
    {
        targets.Clear();
        _centeredTargets.Clear();
        var allMonos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allMonos)
            if (mb is ICamcorderTarget t && !targets.Contains(t)) targets.Add(t);
    }

    private void CheckTargets()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camcorderCamera);
        bool anyCentered = false;

        foreach (var target in targets)
        {
            bool wasCentered = _centeredTargets.Contains(target);
            bool nowCentered = false;

            if (target.IsActive)
            {
                Vector3 worldPos = target.TargetTransform.position;
                float dist = Vector3.Distance(camcorderCamera.transform.position, worldPos);

                if (dist <= maxDetectionDistance &&
                    IsInFrustum(frustumPlanes, worldPos) &&
                    IsCentered(worldPos))
                    nowCentered = true;
            }

            if (nowCentered && !wasCentered)
            {
                _centeredTargets.Add(target);
                (target as ICenteredAware)?.OnCenteredChanged(true);
            }
            else if (!nowCentered && wasCentered)
            {
                _centeredTargets.Remove(target);
                (target as ICenteredAware)?.OnCenteredChanged(false);
            }

            if (nowCentered) anyCentered = true;
        }

        SetState(anyCentered);
    }

    private void ClearAllCentered()
    {
        foreach (var t in _centeredTargets)
            (t as ICenteredAware)?.OnCenteredChanged(false);
        _centeredTargets.Clear();
    }

    private bool IsInFrustum(Plane[] planes, Vector3 worldPos)
    {
        foreach (var plane in planes)
            if (plane.GetDistanceToPoint(worldPos) < 0) return false;
        return true;
    }

    private bool IsCentered(Vector3 worldPos)
    {
        Vector3 viewportPos = camcorderCamera.WorldToViewportPoint(worldPos);
        if (viewportPos.z < 0) return false;

        float distFromCenter = Vector2.Distance(
            new Vector2(viewportPos.x, viewportPos.y),
            new Vector2(0.5f, 0.5f)
        );
        return distFromCenter <= centerThreshold;
    }

    private void SetState(bool isGreen)
    {
        if (currentState == isGreen) return;
        currentState = isGreen;
        GameEvents.CamcorderLightChanged(isGreen);
    }

    private void OnDrawGizmos()
    {
        if (camcorderCamera == null) return;

        // Esfera de detección máxima
        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(camcorderCamera.transform.position, maxDetectionDistance);

        // Borde de la esfera
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(camcorderCamera.transform.position, maxDetectionDistance);

        // Targets detectados — rojo si no centrado, verde si centrado
        foreach (var target in targets)
        {
            if (target?.TargetTransform == null) continue;

            bool isCentered = _centeredTargets.Contains(target);
            Gizmos.color = isCentered ? Color.green : Color.red;
            Gizmos.DrawWireSphere(target.TargetTransform.position, 0.2f);
            Gizmos.DrawLine(camcorderCamera.transform.position, target.TargetTransform.position);
        }
    }
}