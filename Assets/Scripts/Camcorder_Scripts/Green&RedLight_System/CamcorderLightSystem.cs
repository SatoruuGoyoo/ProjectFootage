using System.Collections.Generic;
using UnityEngine;

public class CamcorderLightSystem : MonoBehaviour
{
    public static CamcorderLightSystem Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Reference to the camcorder camera")]
    [SerializeField] private Camera camcorderCamera;

    [Header("Config")]
    [Tooltip("Maximum angle in degrees from the camera's forward to detect a target")]
    [SerializeField] private float maxAimAngle = 18f;
    [Tooltip("Maximum distance in meters to detect a target")]
    [SerializeField] private float maxDetectionDistance = 10f;

    [Header("LayerMask")]


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
                    IsAimedAt(worldPos, target.DetectionRadius))
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

    private bool IsAimedAt(Vector3 worldPos, float radius)
    {
        Vector3 toTarget = worldPos - camcorderCamera.transform.position;
        float dist = toTarget.magnitude;
        if (dist < 0.0001f) return true;

        float angle = Vector3.Angle(camcorderCamera.transform.forward, toTarget);

        float radiusAngle = Mathf.Atan2(radius, dist) * Mathf.Rad2Deg;
        return angle <= maxAimAngle + radiusAngle;
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

        Gizmos.color = new Color(0f, 1f, 0f, 0.15f);
        Gizmos.DrawSphere(camcorderCamera.transform.position, maxDetectionDistance);
        Gizmos.color = new Color(0f, 1f, 0f, 0.8f);
        Gizmos.DrawWireSphere(camcorderCamera.transform.position, maxDetectionDistance);

        Vector3 origin = camcorderCamera.transform.position;
        Vector3 fwd = camcorderCamera.transform.forward;
        Gizmos.color = new Color(1f, 1f, 0f, 0.9f);
        int segments = 24;
        Vector3 prev = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments * 360f;
            Quaternion rot = Quaternion.AngleAxis(maxAimAngle, camcorderCamera.transform.up);
            Vector3 dir = Quaternion.AngleAxis(t, fwd) * (rot * fwd);
            Vector3 point = origin + dir * maxDetectionDistance;
            if (i > 0) Gizmos.DrawLine(prev, point);
            Gizmos.DrawLine(origin, point);
            prev = point;
        }

        foreach (var target in targets)
        {
            if (target?.TargetTransform == null) continue;
            bool aimed = _centeredTargets.Contains(target);
            Gizmos.color = aimed ? Color.green : Color.red;
            Gizmos.DrawWireSphere(target.TargetTransform.position, 0.2f);
            Gizmos.DrawLine(camcorderCamera.transform.position, target.TargetTransform.position);
        }
    }
}