using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CamcorderLightSystem : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Reference to the camcorder camera")]
    [SerializeField] private Camera camcorderCamera;

    [Header("Config")]
    [Tooltip("How close the target needs to be in the green zone to trigger an event")]
    [SerializeField] private float centerThreshold = 0.1f; // How close to the center the target needs to be to be considered 'green'

    private ICamcorderTarget[] targets;
    private bool currentState = false; // false = red, true = green
    private bool isCameraUp = false;

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        isCameraUp = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;  //|| newMode == PlayerMode.MenuCameraMode;
        Debug.Log($"[CamcorderLight] Mode: {newMode} | isCameraUp: {isCameraUp}");

        if (!isCameraUp)
        {
            // Set to red when camera is put down
            SetState(false);
        }
    }

    private void Start()
    {
        RefreshTargets();
    }

    private void Update()
    {
        if(!isCameraUp) return;
        //Debug.Log($"[CamcorderLight] Checking — targets: {targets.Length}");
        CheckTargets();
    }

    public void RefreshTargets()
    {
        var found = FindObjectsByType<CamcorderKeyTarget>(FindObjectsSortMode.None);
        targets = new ICamcorderTarget[found.Length];
        for (int i = 0; i < found.Length; i++)
            targets[i] = found[i];
    }

    private void CheckTargets()
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camcorderCamera);

        foreach (var target in targets)
        {
            if (!target.IsActive) continue;

            // First check: is the target inside the frustum at all?
            Vector3 worldPos = target.TargetTransform.position;
            if (!IsInFrustum(frustumPlanes, worldPos)) continue;

            // Second check: is it centered enough in the viewport?
            if (IsCentered(worldPos))
            {
                SetState(true);
                return;
            }
        }

        SetState(false);
    }

    // Checks if a world position is inside the camera frustum
    private bool IsInFrustum(Plane[] planes, Vector3 worldPos)
    {
        foreach (var plane in planes)
            if (plane.GetDistanceToPoint(worldPos) < 0) return false;
        return true;
    }

    // Checks if a world position is close enough to the center of the viewport
    private bool IsCentered(Vector3 worldPos)
    {
        Vector3 viewportPos = camcorderCamera.WorldToViewportPoint(worldPos);

        // viewportPos.z < 0 means it's behind the camera
        if (viewportPos.z < 0) return false;

        // Viewport center is (0.5, 0.5) / check distance from center
        float distFromCenter = Vector2.Distance(
            new Vector2(viewportPos.x, viewportPos.y),
            new Vector2(0.5f, 0.5f)
        );

        return distFromCenter <= centerThreshold;
    }

    private void SetState(bool isGreen)
    {
        if (currentState == isGreen) return; // no change, don't fire event
        currentState = isGreen;
        GameEvents.CamcorderLightChanged(isGreen);
        Debug.Log($"[CamcorderLight] {(isGreen ? "GREEN" : "RED")}");
    }

}
