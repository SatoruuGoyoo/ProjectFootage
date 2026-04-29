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
    public float slamDuration = 0.12f;
    public float delay = 0.0f;
    public AnimationCurve slamCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Audio (FMOD)")]
    [Tooltip("Evento FMOD del golpe de puerta")]
    public EventReference slamEvent;
    [Tooltip("Transform desde donde suena (dejá null para usar this.transform)")]
    public Transform audioOrigin;

    [Header("Rotación de cierre")]
    [Tooltip("Euler LOCAL de cada puerta en estado cerrado (mismo orden que el Inspector)")]
    public Vector3[] closedEulers; // ← nuevo: llenás en el Inspector, ej: (-90, 0, 0

    // ── privado ─────────────────────────────────────────────
    private bool _ready = false;   // true cuando la iteración correcta llegó
    private bool _played = false;

    // ───────────────────────────────────────────────────────
    private void Start()
    {
        if (zoneTrigger != null)
        {
            zoneTrigger.OnPlayerEntered += OnZoneEntered;
            zoneTrigger.gameObject.SetActive(false); // desactivado al inicio
        }
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
        Debug.Log($"[DoorSlam] iteration: {iteration} target: {targetIteration}");
        if (iteration == targetIteration)
        {
            _ready = true;
            if (zoneTrigger != null)
                zoneTrigger.gameObject.SetActive(true); // activalo cuando llegue la iteración
        }
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

        // Capturamos rotación inicial como Quaternion (sin recomposición)
        var startRots = new Quaternion[doors.Length];
        var endRots = new Quaternion[doors.Length];

        for (int i = 0; i < doors.Length; i++)
        {
            startRots[i] = doors[i].localRotation;
            // Usamos los eulers que vos definís, no los que Unity recalcula
            endRots[i] = Quaternion.Euler(closedEulers[i]);
            Debug.Log($"[DoorSlam] Puerta {i} — start: {startRots[i].eulerAngles}  end: {closedEulers[i]}");
        }

        float elapsed = 0f;
        while (elapsed < slamDuration)
        {
            elapsed += Time.deltaTime;
            float t = slamCurve.Evaluate(Mathf.Clamp01(elapsed / slamDuration));

            for (int i = 0; i < doors.Length; i++)
                doors[i].localRotation = Quaternion.Slerp(startRots[i], endRots[i], t);

            yield return null;
        }

        // Snap final exacto
        for (int i = 0; i < doors.Length; i++)
            doors[i].localRotation = endRots[i];

        if (!slamEvent.IsNull)
        {
            Transform origin = audioOrigin != null ? audioOrigin : transform;
            RuntimeManager.PlayOneShot(slamEvent, origin.position);
        }

        Debug.Log("[DoorSlam] Puertas cerradas.");
    }
}