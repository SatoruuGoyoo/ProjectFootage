using FMODUnity;
using UnityEngine;

public class RecordableAudioSource : MonoBehaviour, ISpatialAudioSource
{
    [Header("FMOD")]
    public EventReference audioEvent;

    [Header("Spatial config")]
    public bool is3D = true;
    [Min(0f)]
    public float maxAudibleDistance = 20f;
    [SerializeField]
    private AnimationCurve distanceFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    public Vector3 WorldPosition => transform.position;
    public string FMODPath => audioEvent.Path;
    public bool Is3D => is3D;
    public float MaxAudibleDistance => maxAudibleDistance;
    public bool IsActiveInScene => gameObject.activeInHierarchy && !audioEvent.IsNull;

    public bool TryGetTimelinePosition(out int milliseconds)
    {
        milliseconds = 0;
        return false;
    }

    public float EvaluateDistanceFalloff(float normalizedDistance)
        => distanceFalloff.Evaluate(normalizedDistance);

    // ── Gizmos ─────────────────────────────────────────────────

    private static readonly Color GizmoColor = new Color(0.4f, 0.8f, 1f); // celeste

    private void OnDrawGizmos()
    {
        // Siempre visible — solo el wire para no molestar
        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.5f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);
    }

    private void OnDrawGizmosSelected()
    {
        // Cuando está seleccionado, wire más brillante + esfera traslúcida
        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 1f);
        Gizmos.DrawWireSphere(transform.position, maxAudibleDistance);

        Gizmos.color = new Color(GizmoColor.r, GizmoColor.g, GizmoColor.b, 0.08f);
        Gizmos.DrawSphere(transform.position, maxAudibleDistance);

        // Marcador del centro para ubicar la fuente exacta
        Gizmos.color = GizmoColor;
        Gizmos.DrawSphere(transform.position, 0.15f);
    }
}