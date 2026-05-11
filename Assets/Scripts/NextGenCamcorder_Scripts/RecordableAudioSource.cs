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

    // Sin Start(), sin instancia FMOD, sin OnDestroy().
    // Este componente es solo un descriptor — dice "acá hay una fuente de audio
    // con estas propiedades". El sonido solo existe en el playback.

    public Vector3 WorldPosition => transform.position;
    public string FMODPath => audioEvent.Path;
    public bool Is3D => is3D;
    public float MaxAudibleDistance => maxAudibleDistance;
    public bool IsActiveInScene => gameObject.activeInHierarchy && !audioEvent.IsNull;

    public bool TryGetTimelinePosition(out int milliseconds)
    {
        milliseconds = 0;
        return false; // no hay instancia corriendo, el playback arranca desde 0
    }

    public float EvaluateDistanceFalloff(float normalizedDistance)
        => distanceFalloff.Evaluate(normalizedDistance);
}