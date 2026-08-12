using System;
using System.Collections.Generic;
using UnityEngine;

public enum CamcorderZone
{
    None,
    Objective,
    DeadZone
}

public class CamcorderDetectionSystem : MonoBehaviour
{
    public static CamcorderDetectionSystem Instance { get; private set; }

    [Header("Setup")]
    [Tooltip("Reference to the camcorder camera (uses its position/forward for the cylinder axis)")]
    [SerializeField] private Camera camcorderCamera;

    [Header("Cylinder Config")]
    [Tooltip("Radius of the objective zone (green). Target inside this = GREEN.")]
    [SerializeField] private float objectiveRadius = 0.8f;
    [Tooltip("Radius of the dead zone (red). Between objective and this = RED. Outside this = nothing.")]
    [SerializeField] private float deadZoneRadius = 2.5f;
    [Tooltip("Minimum detection distance (independent of the camera's near clip plane).")]
    [SerializeField] private float detectionMinDistance = 0.3f;
    [Tooltip("Maximum detection distance (independent of the camera's far clip plane). Raising the camera's far no longer affects this.")]
    [SerializeField] private float detectionMaxDistance = 15f;

    [Header("Line of Sight")]
    [Tooltip("If enabled, requires a clear line of sight between the camera and the target.")]
    [SerializeField] private bool requireLineOfSight = true;
    [Tooltip("Layers that block line of sight (walls, doors, etc.).")]
    [SerializeField] private LayerMask losBlockingMask = ~0;

    [Header("Gizmo")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color objectiveColor = new Color(0f, 1f, 0f, 0.4f);
    [SerializeField] private Color deadZoneColor = new Color(1f, 0f, 0f, 0.25f);
    [SerializeField] private int gizmoSegments = 24;

    private readonly List<ICamcorderTarget> _targets = new();
    private CamcorderZone _currentZone = CamcorderZone.None;
    private bool _isCameraUp = false;

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

    private void Start() => RefreshTargets();

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        _isCameraUp = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;

        if (!_isCameraUp)
            SetZone(CamcorderZone.None);
    }

    private void Update()
    {
        if (!_isCameraUp) return;
        EvaluateZone();
    }

    public void Register(ICamcorderTarget target)
    {
        if (!_targets.Contains(target))
        {
            _targets.Add(target);
            Debug.Log($"[Detection] Registered: {((MonoBehaviour)target).gameObject.name}");
        }
    }

    public void Unregister(ICamcorderTarget target)
    {
        _targets.Remove(target);
    }

    public void RefreshTargets()
    {
        _targets.Clear();
        var allMonos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var mb in allMonos)
            if (mb is ICamcorderTarget t && !_targets.Contains(t)) _targets.Add(t);
    }

    public CamcorderZone GetZoneForTarget(ICamcorderTarget target)
    {
        if (!_isCameraUp) return CamcorderZone.None;
        if (camcorderCamera == null) return CamcorderZone.None;
        if (target == null || !target.IsActive || target.TargetTransform == null) return CamcorderZone.None;

        return GetZoneForPoint(target.TargetTransform.position);
    }

    private void EvaluateZone()
    {
        if (camcorderCamera == null)
        {
            SetZone(CamcorderZone.None);
            return;
        }

        CamcorderZone bestZone = CamcorderZone.None;

        foreach (var target in _targets)
        {
            if (target == null || !target.IsActive || target.TargetTransform == null) continue;

            CamcorderZone zone = GetZoneForPoint(target.TargetTransform.position);

            if (zone == CamcorderZone.Objective)
            {
                bestZone = CamcorderZone.Objective;
                break;
            }
            else if (zone == CamcorderZone.DeadZone && bestZone == CamcorderZone.None)
            {
                bestZone = CamcorderZone.DeadZone;
            }
        }

        SetZone(bestZone);
    }

    private CamcorderZone GetZoneForPoint(Vector3 worldPos)
    {
        Vector3 camPos = camcorderCamera.transform.position;
        Vector3 camForward = camcorderCamera.transform.forward;

        Vector3 toPoint = worldPos - camPos;

        float axialDistance = Vector3.Dot(toPoint, camForward);

        if (axialDistance < detectionMinDistance) return CamcorderZone.None;
        if (axialDistance > detectionMaxDistance) return CamcorderZone.None;

        Vector3 axialPoint = camPos + camForward * axialDistance;
        float radialDistance = Vector3.Distance(worldPos, axialPoint);

        CamcorderZone zone;
        if (radialDistance <= objectiveRadius) zone = CamcorderZone.Objective;
        else if (radialDistance <= deadZoneRadius) zone = CamcorderZone.DeadZone;
        else return CamcorderZone.None;

        if (requireLineOfSight)
        {
            Vector3 dir = worldPos - camPos;
            float dist = dir.magnitude;
            if (Physics.Raycast(camPos, dir.normalized, dist, losBlockingMask, QueryTriggerInteraction.Ignore))
                return CamcorderZone.None;
        }

        return zone;
    }

    private void SetZone(CamcorderZone zone)
    {
        if (_currentZone == zone) return;
        _currentZone = zone;
        Debug.Log($"[Detection] Zona cambió a: {zone}");
        GameEvents.ZoneChanged(_currentZone);
    }

    public CamcorderZone CurrentZone => _currentZone;

    private void OnDrawGizmos()
    {
        if (!drawGizmos || camcorderCamera == null) return;

        Vector3 camPos = camcorderCamera.transform.position;
        Vector3 camForward = camcorderCamera.transform.forward;
        Vector3 camUp = camcorderCamera.transform.up;
        Vector3 camRight = camcorderCamera.transform.right;

        float nearZ = detectionMinDistance;
        float farZ = detectionMaxDistance;

        Vector3 nearCenter = camPos + camForward * nearZ;
        Vector3 farCenter = camPos + camForward * farZ;

        DrawCylinderWire(nearCenter, farCenter, camRight, camUp, objectiveRadius, objectiveColor);
        DrawCylinderWire(nearCenter, farCenter, camRight, camUp, deadZoneRadius, deadZoneColor);
    }

    private void DrawCylinderWire(Vector3 a, Vector3 b, Vector3 right, Vector3 up, float radius, Color color)
    {
        Gizmos.color = color;

        Vector3 prevNear = a + right * radius;
        Vector3 prevFar = b + right * radius;

        for (int i = 1; i <= gizmoSegments; i++)
        {
            float angle = (float)i / gizmoSegments * Mathf.PI * 2f;
            Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

            Vector3 currNear = a + offset;
            Vector3 currFar = b + offset;

            Gizmos.DrawLine(prevNear, currNear);
            Gizmos.DrawLine(prevFar, currFar);

            if (i % (gizmoSegments / 4) == 0)
                Gizmos.DrawLine(currNear, currFar);

            prevNear = currNear;
            prevFar = currFar;
        }
    }
}