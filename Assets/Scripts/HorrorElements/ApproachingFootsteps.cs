using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
public class ApproachingFootsteps : MonoBehaviour
{
    [Header("Trigger")]
    public HorrorZoneTrigger zoneTrigger;

    [Header("Audio")]
    public AudioClip footstepClip;
    public AudioClip arrivalClip;


    [Header("Movimiento")]
    public Transform player;
    public float startDistance = 22f;
    public float startSpeed = 2.5f;
    public float endSpeed = 11f;
    public float arrivalDistance = 1.2f;
    [Tooltip("Dirección desde la que vienen los pasos en world space (se normaliza solo)")]
    public Vector3 approachDirection = Vector3.forward;

    [Header("Timing")]
    public float delay = 0.5f;
    public float approachDuration = 5f;   // ← cuántos segundos dura la aproximación
    public float arrivalPause = 0.12f;

    [Header("Progresión de sonido")]
    [Tooltip("Frecuencia de corte del Low Pass al inicio (muy lejos, apagado). ~400–800 Hz")]
    public float cutoffFreqFar = 500f;
    [Tooltip("Frecuencia de corte al llegar (sonido nítido y seco). 22000 = sin filtro")]
    public float cutoffFreqNear = 22000f;
    [Tooltip("Pitch al inicio (pasos lentos)")]
    public float pitchStart = 0.85f;
    [Tooltip("Pitch al llegar (carrera, urgencia)")]
    public float pitchEnd = 1.3f;
    [Tooltip("Volumen al inicio (casi inaudible)")]
    public float volumeStart = 0.05f;
    [Tooltip("Volumen al llegar")]
    public float volumeEnd = 1f;

    // ── privado ─────────────────────────────────────────────────────
    private AudioSource _src;
    private AudioLowPassFilter _lpf;
    private bool _played = false;

    // ───────────────────────────────────────────────────────────────
    private void Awake()
    {
        _src = GetComponent<AudioSource>();
        _lpf = GetComponent<AudioLowPassFilter>();
        ConfigureAudioSource();
    }

    private void Start()
    {
        if (zoneTrigger != null)
            zoneTrigger.OnPlayerEntered += Play;
    }

    private void OnDestroy()
    {
        if (zoneTrigger != null)
            zoneTrigger.OnPlayerEntered -= Play;
    }

    // ── API pública ─────────────────────────────────────────────────
    public void Play()
    {
        if (_played) return;
        _played = true;
        StartCoroutine(RunSequence());
    }

    // ── Secuencia ───────────────────────────────────────────────────
    private IEnumerator RunSequence()
    {
        if (player == null) { Debug.LogError("[ApproachingFootsteps] player no asignado."); yield break; }

        // ── Posición inicial: lejos en la dirección configurada ──────
        Vector3 dir = approachDirection.normalized;
        Vector3 start = player.position + dir * startDistance;
        start.y = player.position.y;
        transform.position = start;

        // ── Estado de audio inicial ──────────────────────────────────
        _src.volume = volumeStart;
        _src.pitch = pitchStart;
        _lpf.cutoffFrequency = cutoffFreqFar;

        // ── Delay inicial ────────────────────────────────────────────
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        // ── Arrancar pasos ───────────────────────────────────────────
        _src.clip = footstepClip;
        _src.loop = true;
        _src.Play();

        
        float elapsed = 0f;

        while (true)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= arrivalDistance) break;

            elapsed += Time.deltaTime;

            // t basado en tiempo, no en distancia — así controlás la duración exacta
            float t = Mathf.Clamp01(elapsed / approachDuration);
            float tCurved = t * t;

            float speed = Mathf.Lerp(startSpeed, endSpeed, tCurved);
            transform.position = Vector3.MoveTowards(
                transform.position, player.position, speed * Time.deltaTime);

            var pos = transform.position; pos.y = player.position.y;
            transform.position = pos;

            _lpf.cutoffFrequency = Mathf.Lerp(cutoffFreqFar, cutoffFreqNear, tCurved);
            _src.pitch = Mathf.Lerp(pitchStart, pitchEnd, tCurved);
            _src.volume = Mathf.Lerp(volumeStart, volumeEnd, t);

            yield return null;
        }

        // ── Llegada ──────────────────────────────────────────────────
        _src.Stop();
        _lpf.cutoffFrequency = cutoffFreqNear; // sin filtro para el clip de llegada

        if (arrivalPause > 0f) yield return new WaitForSecondsRealtime(arrivalPause);

        if (arrivalClip != null)
            _src.PlayOneShot(arrivalClip, volumeEnd);

        Debug.Log("[ApproachingFootsteps] Secuencia completada.");
    }

    // ── Config AudioSource ──────────────────────────────────────────
    private void ConfigureAudioSource()
    {
        _src.spatialBlend = 1f;
        _src.rolloffMode = AudioRolloffMode.Custom;
        _src.minDistance = 1f;
        _src.maxDistance = 30f;
        _src.dopplerLevel = 0.8f;
        _src.playOnAwake = false;
        _src.loop = false;

        // Rolloff: muy silencioso lejos, sube agresivamente al acercarse
        var rolloff = new AnimationCurve(
            new Keyframe(0f, 1f, 0f, 0f),
            new Keyframe(0.05f, 0.9f),
            new Keyframe(0.15f, 0.55f),
            new Keyframe(0.35f, 0.15f),
            new Keyframe(1f, 0f, 0f, 0f)
        );
        _src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloff);

        // LPF estado inicial: muy apagado
        _lpf.cutoffFrequency = cutoffFreqFar;
        _lpf.lowpassResonanceQ = 1f;
    }
}