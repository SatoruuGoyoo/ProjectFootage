using FMODUnity;
using UnityEngine;

public class RecordableAudioSource : MonoBehaviour, ISpatialAudioSource
{
    [Header("FMOD")]
    [SerializeField] private EventReference audioEvent;

    [Header("Spatial config")]
    [SerializeField] private bool is3D = true;
    [Min(0f)]
    [SerializeField] private float maxAudibleDistance = 20f;
    [SerializeField]
    private AnimationCurve distanceFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    public Vector3 WorldPosition => transform.position;
    public string FMODPath => audioEvent.Guid.ToString();
    public bool Is3D => is3D;
    public float MaxAudibleDistance => maxAudibleDistance;
    public bool IsActiveInScene => gameObject.activeInHierarchy && !audioEvent.IsNull;

    public void StartCapture() => SpatialAudioRecorder.TryRegisterDynamic(this);
    public void StopCapture() => SpatialAudioRecorder.TryUnregisterDynamic(this);

    public bool TryGetTimelinePosition(out int milliseconds)
    {
        milliseconds = 0;
        return false;
    }

    public float EvaluateDistanceFalloff(float normalizedDistance)
        => distanceFalloff.Evaluate(normalizedDistance);


    private static readonly Color GizmoColor = new Color(0.4f, 0.8f, 1f);

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);

        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.08f);
        Gizmos.DrawSphere(transform.position, maxAudibleDistance);

        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}