using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public enum CameraMode
{
    Static,
    Follow,
    LookAt
}

[RequireComponent(typeof(Camera))]
public class FixedCameraController : MonoBehaviour
{
    // Mode
    [Header("Camera Mode")]
    [Tooltip("Tweak between different camera modes")]
    [SerializeField] private CameraMode cameraMode = CameraMode.Static;

    // LookAt Mode
    [Header("LookAt Mode Settings")]
    [Tooltip("Range of vertical rotation (pitch)")]
    public Vector2 pitchRange = new(-20, 20f);

    [Tooltip("Range of horizontal rotation (yaw) ")]
    public Vector2 yawRange = new(-45f, 45f);

    [Tooltip("Speed at which the camera looks at the target ")]
    public float lookSpeed = 6f;

    public bool clampRotation = true;

    // Follow Mode
    [Header("Follow / Waypoints")]
    [Tooltip("Waypoints")]
    public Transform[] waypoints;

    [Header("Follow player")]
    [Tooltip("Offset")]
    public Vector3 followOffset = Vector3.zero;

    [Tooltip("Follow speed")]
    public float followSpeed = 5f;
    [Tooltip("If ON, the camera looks at the player while following the path")]
    public bool lookAtPlayerWhileFollowing = true;

    private Transform _player;
    private bool _justActivated;
    private Quaternion _initialLocalRot;
    private float _initPitch, _initYaw, _initRoll;

    private void Awake()
    {
        _initialLocalRot = transform.localRotation;
        Vector3 e = _initialLocalRot.eulerAngles;
        _initPitch = NormalizeAngle(e.x);
        _initYaw = NormalizeAngle(e.y);
        _initRoll = NormalizeAngle(e.z);
    }

    private void LateUpdate()
    {
        if (!TryGetPlayer()) return;

        switch (cameraMode)
        {
            case CameraMode.LookAt: HandleLookAt(); break;
            case CameraMode.Follow: HandleFollow(); break;
        }
    }

    public void OnActivated()
    {
        _justActivated = true;
        if (cameraMode == CameraMode.LookAt)
            transform.localRotation = _initialLocalRot;
        
    }

    private void HandleLookAt()
    {
        Vector3 dir = _player.position - transform.position;
        if (dir == Vector3.zero) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        Vector3 euler = targetRot.eulerAngles;

        float pitch = NormalizeAngle(euler.x);
        float yaw = NormalizeAngle(euler.y);

        if (clampRotation)
        {
            pitch = Mathf.Clamp(pitch, _initPitch + pitchRange.x, _initPitch + pitchRange.y);
            yaw = Mathf.Clamp(yaw, _initYaw + yawRange.x, _initYaw + yawRange.y);
        }

        Quaternion clamped = Quaternion.Euler(pitch, yaw, _initRoll);

        if(_justActivated)
        {
            transform.rotation = clamped;
            _justActivated = false;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, clamped, Time.deltaTime * lookSpeed);
        }
        
    }

    private void HandleFollow()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector3 targetPos = GetClosestPointOnPath(_player.position) + followOffset;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);

        if (lookAtPlayerWhileFollowing)
        {
            Vector3 dir = _player.position - transform.position;
            if (dir != Vector3.zero)
            {
                Quaternion look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * followSpeed);
            }
        }
    }

    private Vector3 GetClosestPointOnPath(Vector3 point)
    {
        if (waypoints.Length == 1) return waypoints[0].position;

        float closest = float.MaxValue;
        Vector3 closestPoint = waypoints[0].position;

        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            Vector3 candidate = ProjectOnSegment(point, waypoints[i].position, waypoints[i + 1].position);
            float dist = Vector3.Distance(point, candidate);
            if (dist < closest) { closest = dist; closestPoint = candidate; }
        }

        return closestPoint;
    }

    private static Vector3 ProjectOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Mathf.Clamp01(Vector3.Dot(p - a, ab) / ab.sqrMagnitude);
        return a + t * ab;
    }

    private static float NormalizeAngle(float angle) =>
       angle > 180f ? angle - 360f : angle;

    private bool TryGetPlayer()
    {
        if (_player != null) return true;
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go) _player = go.transform;
        return _player != null;
    }



}
