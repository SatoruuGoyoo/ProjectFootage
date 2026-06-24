using FMODUnity;
using UnityEngine;

public class RecordableOneShotEmitter : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference fmodEvent;

    [Header("Spatial config")]
    [Min(0f)]
    [SerializeField] private float maxAudibleDistance = 20f;

    public void Emit()
    {
        if (fmodEvent.IsNull) return;
        SpatialAudioRecorder.TryCaptureOneShot(fmodEvent, transform.position, maxAudibleDistance);
    }

    private static readonly Color GizmoColor = new(1f, 0.55f, 0.1f);

    private void OnDrawGizmos()
    {
        Gizmos.color = new(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new(GizmoColor.r, GizmoColor.g, GizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);

        Gizmos.color = new(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.08f);
        Gizmos.DrawSphere(transform.position, maxAudibleDistance);

        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}