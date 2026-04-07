using UnityEngine;

/// <summary>
/// Colocalo en el mismo GameObject que el AudioSource de la escena que querés
/// que se escuche "bien" solo cuando la cámara estaba cerca y alineada.
///
/// Durante la grabación: CamcorderRecorder llama a SampleVolume() cada frame
/// capturado y guarda el resultado como metadato.
///
/// Durante la reproducción: CamcorderPlayback llama a ApplyPlaybackVolume()
/// con el valor guardado para ese frame, modulando el AudioSource en tiempo real.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CamcorderDiegeticAudio : MonoBehaviour
{
    [Header("Proximity")]
    [Tooltip("Distancia máxima a la que la cámara puede escuchar este audio.")]
    [SerializeField] private float maxDistance = 8f;

    [Tooltip("Curva de volumen según distancia normalizada (0 = junto al objeto, 1 = maxDistance).")]
    [SerializeField] private AnimationCurve distanceCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Alignment")]
    [Tooltip("Dot product mínimo para que se escuche algo. 1 = perfecto enfrente, 0 = 90°. " +
             "Recomendado: 0.6–0.8 para un cono medio-estrecho.")]
    [SerializeField, Range(0f, 1f)] private float minDotThreshold = 0.65f;

    [Tooltip("Dot product a partir del cual el volumen es máximo. Debe ser > minDotThreshold.")]
    [SerializeField, Range(0f, 1f)] private float fullVolumeDot = 0.9f;

    // ── Refs ───────────────────────────────────────────────
    private AudioSource audioSource;

    // Volumen base en escena (sin playback activo)
    private float baseVolume;

    // ── Unity ──────────────────────────────────────────────
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        baseVolume = audioSource.volume;
    }

    // ── API pública ────────────────────────────────────────

    /// <summary>
    /// Llamado por CamcorderRecorder cada vez que captura un frame.
    /// Devuelve un valor 0-1 que representa qué tan bien la cámara
    /// estaba posicionada y apuntando a este audio source.
    /// </summary>
    public float SampleVolume(Vector3 camPosition, Vector3 camForward)
    {
        // ─ Proximidad ─
        float distance = Vector3.Distance(camPosition, transform.position);
        if (distance >= maxDistance) return 0f;

        float normalizedDist = distance / maxDistance;
        float proxFactor = distanceCurve.Evaluate(normalizedDist);

        // ─ Alineación ─
        Vector3 toSource = (transform.position - camPosition).normalized;
        float dot = Vector3.Dot(camForward, toSource);

        if (dot < minDotThreshold) return 0f;

        float alignFactor = Mathf.InverseLerp(minDotThreshold, fullVolumeDot, dot);
        alignFactor = Mathf.Clamp01(alignFactor);

        return proxFactor * alignFactor;
    }

    /// <summary>
    /// Llamado por CamcorderPlayback frame a frame durante la reproducción.
    /// Recibe el valor 0-1 muestreado al momento de la grabación.
    /// </summary>
    public void ApplyPlaybackVolume(float sampledVolume)
    {
        audioSource.volume = baseVolume * sampledVolume;
    }

    /// <summary>
    /// Restaura el volumen base al terminar/interrumpir la reproducción.
    /// </summary>
    public void ResetVolume()
    {
        audioSource.volume = baseVolume;
    }
}
