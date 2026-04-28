using System.Collections;
using UnityEngine;
using FMODUnity;

/// <summary>
/// En la iteración configurada, al entrar en zona, todas las puertas
/// asignadas rotan en Z hasta 0 de golpe (slam).
/// </summary>
public class DoorSlam : MonoBehaviour
{
    [Header("Iteración")]
    [Tooltip("0 = primera iteración, 1 = segunda, etc.")]
    public int targetIteration = 1;

    [Header("Trigger")]
    public HorrorZoneTrigger zoneTrigger;

    [Header("Puertas")]
    [Tooltip("Cada Transform es una puerta. Todas se cierran a la vez.")]
    public Transform[] doors;

    [Header("Animación")]
    [Tooltip("Segundos que tarda el slam (corto = más violento)")]
    public float slamDuration = 0.12f;
    [Tooltip("Delay entre que entra al trigger y el golpe")]
    public float delay = 0.0f;
    [Tooltip("Curva de easing — recomendado: arranque rápido, frenada brusca")]
    public AnimationCurve slamCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio (FMOD)")]
    [Tooltip("Evento FMOD del golpe de puerta")]
    public EventReference slamEvent;
    [Tooltip("Transform desde donde suena (dejá null para usar this.transform)")]
    public Transform audioOrigin;

    // ── privado ─────────────────────────────────────────────
    private bool _ready = false;   // true cuando la iteración correcta llegó
    private bool _played = false;

    // ───────────────────────────────────────────────────────
    private void Start()
    {
        if (zoneTrigger != null)
            zoneTrigger.OnPlayerEntered += OnZoneEntered;

        GameEvents.OnIterationChanged += OnIterationChanged;
    }

    private void OnDestroy()
    {
        if (zoneTrigger != null)
            zoneTrigger.OnPlayerEntered -= OnZoneEntered;

        GameEvents.OnIterationChanged -= OnIterationChanged;
    }

    // ── Callbacks ────────────────────────────────────────────
    private void OnIterationChanged(int iteration)
    {
        if (iteration == targetIteration)
            _ready = true;
    }

    private void OnZoneEntered()
    {
        if (!_ready || _played) return;
        _played = true;
        StartCoroutine(SlamSequence());
    }

    // ── Secuencia ────────────────────────────────────────────
    private IEnumerator SlamSequence()
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        // Capturamos rotación inicial de cada puerta
        var startAngles = new Vector3[doors.Length];
        for (int i = 0; i < doors.Length; i++)
            startAngles[i] = doors[i].localEulerAngles;

        float elapsed = 0f;

        while (elapsed < slamDuration)
        {
            elapsed += Time.deltaTime;
            float t = slamCurve.Evaluate(Mathf.Clamp01(elapsed / slamDuration));

            for (int i = 0; i < doors.Length; i++)
            {
                var angles = startAngles[i];
                angles.z = Mathf.LerpAngle(startAngles[i].z, 0f, t);
                doors[i].localEulerAngles = angles;
            }

            yield return null;
        }

        // Snap final exacto
        for (int i = 0; i < doors.Length; i++)
        {
            var angles = doors[i].localEulerAngles;
            angles.z = 0f;
            doors[i].localEulerAngles = angles;
        }

        // ── Sonido FMOD ──────────────────────────────────────
        if (!slamEvent.IsNull)
        {
            Transform origin = audioOrigin != null ? audioOrigin : transform;
            RuntimeManager.PlayOneShot(slamEvent, origin.position);
        }

        Debug.Log("[DoorSlam] Puertas cerradas.");
    }
}