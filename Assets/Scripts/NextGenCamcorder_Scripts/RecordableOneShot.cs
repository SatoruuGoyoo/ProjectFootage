using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RecordableOneShot : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference fmodEvent;

    [Header("Spatial config")]
    [Min(0f)]
    [SerializeField] private float maxAudibleDistance = 20f;

    [Header("Trigger config")]
    [SerializeField] private bool fireOnce = true;
    [SerializeField] private string triggerTag = "Player";

    private bool _fired = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(triggerTag)) return;
        if (fireOnce && _fired) return;

        if (SpatialAudioRecorder.TryCaptureOneShot(fmodEvent, transform.position, maxAudibleDistance))
            _fired = true;
    }

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
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