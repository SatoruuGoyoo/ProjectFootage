using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ─── Enums ────────────────────────────────────────────────────────────────────

public enum HorrorTriggerType
{
    OnSceneStart,       // Se dispara automáticamente al inicio de la escena
    ClockSolved,        // GameEvents.OnClockSolved
    IterationChanged,   // GameEvents.OnIterationChanged (filtrado por número)
    PuzzleCompleted,    // GameEvents.OnPuzzleCompleted (puzzle de puertas resuelto)
    SpatialZone         // El jugador entra en un HorrorZoneTrigger
}

public enum HorrorActionType
{
    Apparition,         // El target aparece, opcionalmente se mueve, luego desaparece
    PathMovement,       // El target se mueve por waypoints y queda en el último punto
    SoundOnly           // Reproduce un AudioClip en una posición del espacio
}

// ─── Data ─────────────────────────────────────────────────────────────────────

[Serializable]
public class HorrorEventEntry
{
    [Tooltip("Nombre descriptivo visible en el Inspector.")]
    public string label = "Nuevo Evento";

    // ── Trigger ──────────────────────────────────────────────────────────────
    [Header("Trigger")]
    public HorrorTriggerType triggerType;

    [Tooltip("SpatialZone: referencia al HorrorZoneTrigger que activa este evento.")]
    public HorrorZoneTrigger zoneTrigger;

    [Tooltip("IterationChanged: número de iteración (0, 1, 2...) que dispara el evento.")]
    public int iterationFilter = 0;

    // ── Acción ───────────────────────────────────────────────────────────────
    [Header("Acción")]
    public HorrorActionType actionType;

    [Tooltip("Apparition / PathMovement: GameObject a activar/mover.")]
    public GameObject target;

    [Tooltip("Apparition: segundos que el target permanece visible antes de desaparecer.")]
    public float appearDuration = 3f;

    [Tooltip("Apparition: si true, el target se mueve por los waypoints mientras está visible.")]
    public bool usePath = false;

    [Tooltip("Waypoints del recorrido (Apparition con path / PathMovement).\nEl target se snappea al primero al inicio.")]
    public Transform[] path;

    [Tooltip("Velocidad de movimiento entre waypoints (unidades/seg).")]
    public float pathSpeed = 1.5f;

    [Tooltip("SoundOnly: clip de audio a reproducir.")]
    public AudioClip audioClip;

    [Tooltip("SoundOnly: Transform donde suena el clip. Si es null, suena en la posición del Manager.")]
    public Transform audioPosition;

    [Range(0f, 1f)]
    [Tooltip("SoundOnly: volumen de reproducción.")]
    public float audioVolume = 1f;

    // ── Timing ───────────────────────────────────────────────────────────────
    [Header("Timing")]
    [Tooltip("Segundos de espera antes de ejecutar la acción tras dispararse el trigger.")]
    public float delay = 0f;

    [Tooltip("Si true, el evento solo ocurre una vez por sesión sin importar cuántas veces se dispare el trigger.")]
    public bool oneShot = true;

    [Tooltip("Tiempo mínimo (seg) entre disparos consecutivos. Solo aplica si oneShot = false.")]
    public float cooldown = 10f;

    // ── Runtime (no serializado) ──────────────────────────────────────────────
    [NonSerialized] public bool hasTriggered;
    [NonSerialized] public float lastTriggeredTime = -999f;
}

// ─── Manager ──────────────────────────────────────────────────────────────────

/// <summary>
/// Colocá este componente en un GameObject vacío de la escena.
/// Configurá la lista de Horror Events en el Inspector:
/// cada entrada tiene un Trigger (qué lo activa) y una Acción (qué hace).
/// </summary>
public class HorrorEventManager : MonoBehaviour
{
    [SerializeField] private List<HorrorEventEntry> events = new List<HorrorEventEntry>();

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Start()
    {
        InitializeTargets();
        SubscribeToGameEvents();
        SubscribeToZoneTriggers();
        FireSceneStartEvents();
    }

    private void OnDestroy()
    {
        GameEvents.OnClockSolved      -= OnClockSolved;
        GameEvents.OnIterationChanged -= OnIterationChanged;
        GameEvents.OnPuzzleCompleted  -= OnPuzzleCompleted;
    }

    // ── Inicialización ────────────────────────────────────────────────────────

    private void InitializeTargets()
    {
        foreach (var entry in events)
        {
            // Las apariciones deben empezar ocultas
            if (entry.actionType == HorrorActionType.Apparition && entry.target != null)
                entry.target.SetActive(false);
        }
    }

    private void SubscribeToGameEvents()
    {
        GameEvents.OnClockSolved      += OnClockSolved;
        GameEvents.OnIterationChanged += OnIterationChanged;
        GameEvents.OnPuzzleCompleted  += OnPuzzleCompleted;
    }

    private void SubscribeToZoneTriggers()
    {
        foreach (var entry in events)
        {
            if (entry.triggerType != HorrorTriggerType.SpatialZone) continue;
            if (entry.zoneTrigger == null)
            {
                Debug.LogWarning($"[HorrorEventManager] '{entry.label}': triggerType es SpatialZone pero zoneTrigger no está asignado.");
                continue;
            }

            var capturedEntry = entry;
            entry.zoneTrigger.OnPlayerEntered += () => TryFire(capturedEntry);
        }
    }

    private void FireSceneStartEvents()
    {
        foreach (var entry in events)
            if (entry.triggerType == HorrorTriggerType.OnSceneStart)
                TryFire(entry);
    }

    // ── Listeners de GameEvents ───────────────────────────────────────────────

    private void OnClockSolved()
    {
        foreach (var entry in events)
            if (entry.triggerType == HorrorTriggerType.ClockSolved)
                TryFire(entry);
    }

    private void OnIterationChanged(int iteration)
    {
        foreach (var entry in events)
            if (entry.triggerType == HorrorTriggerType.IterationChanged && entry.iterationFilter == iteration)
                TryFire(entry);
    }

    private void OnPuzzleCompleted()
    {
        foreach (var entry in events)
            if (entry.triggerType == HorrorTriggerType.PuzzleCompleted)
                TryFire(entry);
    }

    // ── Fire ──────────────────────────────────────────────────────────────────

    private void TryFire(HorrorEventEntry entry)
    {
        if (entry.oneShot && entry.hasTriggered) return;

        if (!entry.oneShot && Time.unscaledTime - entry.lastTriggeredTime < entry.cooldown) return;

        entry.hasTriggered = true;
        entry.lastTriggeredTime = Time.unscaledTime;

        StartCoroutine(FireWithDelay(entry));
    }

    private IEnumerator FireWithDelay(HorrorEventEntry entry)
    {
        if (entry.delay > 0f)
            yield return new WaitForSecondsRealtime(entry.delay);

        switch (entry.actionType)
        {
            case HorrorActionType.Apparition:
                StartCoroutine(ExecuteApparition(entry));
                break;
            case HorrorActionType.PathMovement:
                StartCoroutine(ExecutePathMovement(entry.target, entry.path, entry.pathSpeed, loop: false));
                break;
            case HorrorActionType.SoundOnly:
                ExecuteSoundOnly(entry);
                break;
        }
    }

    // ── Acciones ──────────────────────────────────────────────────────────────

    private IEnumerator ExecuteApparition(HorrorEventEntry entry)
    {
        if (entry.target == null)
        {
            Debug.LogWarning($"[HorrorEventManager] '{entry.label}': target no asignado para Apparition.");
            yield break;
        }

        // Snappear al primer waypoint si tiene path
        if (entry.usePath && entry.path != null && entry.path.Length > 0 && entry.path[0] != null)
            entry.target.transform.position = entry.path[0].position;

        entry.target.SetActive(true);

        if (entry.usePath && entry.path != null && entry.path.Length > 1)
        {
            // Mover por el path mientras está visible; parar al cumplirse appearDuration
            Coroutine pathCo = StartCoroutine(ExecutePathMovement(entry.target, entry.path, entry.pathSpeed, loop: false));
            yield return new WaitForSecondsRealtime(entry.appearDuration);
            StopCoroutine(pathCo);
        }
        else
        {
            yield return new WaitForSecondsRealtime(entry.appearDuration);
        }

        if (entry.target != null)
            entry.target.SetActive(false);
    }

    /// <param name="loop">Si true, el path se recorre en loop indefinido (hasta que lo paren externamente).</param>
    private IEnumerator ExecutePathMovement(GameObject target, Transform[] path, float speed, bool loop)
    {
        if (target == null || path == null || path.Length < 2) yield break;

        // Snappear al origen
        target.transform.position = path[0].position;

        int startIndex = 1;
        do
        {
            for (int i = startIndex; i < path.Length; i++)
            {
                if (path[i] == null) continue;
                Vector3 destination = path[i].position;

                while (target != null && target.activeSelf)
                {
                    float dist = Vector3.Distance(target.transform.position, destination);
                    if (dist <= 0.05f) break;

                    target.transform.position = Vector3.MoveTowards(
                        target.transform.position, destination, speed * Time.deltaTime);

                    // Rotar hacia el destino (solo en Y para personajes)
                    Vector3 dir = destination - target.transform.position;
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 0.001f)
                        target.transform.rotation = Quaternion.LookRotation(dir);

                    yield return null;
                }

                if (target == null || !target.activeSelf) yield break;
            }
            startIndex = 1; // En loop, vuelve desde el waypoint [1] (no desde [0])
        }
        while (loop);
    }

    private void ExecuteSoundOnly(HorrorEventEntry entry)
    {
        if (entry.audioClip == null)
        {
            Debug.LogWarning($"[HorrorEventManager] '{entry.label}': audioClip no asignado para SoundOnly.");
            return;
        }

        Vector3 pos = entry.audioPosition != null ? entry.audioPosition.position : transform.position;
        AudioSource.PlayClipAtPoint(entry.audioClip, pos, entry.audioVolume);
    }

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Dispara manualmente un evento por su label.
    /// Útil para conectar desde botones, animaciones o scripts externos via Inspector (UnityEvent).
    /// </summary>
    public void FireEventByLabel(string label)
    {
        var entry = events.Find(e => e.label == label);
        if (entry == null)
        {
            Debug.LogWarning($"[HorrorEventManager] FireEventByLabel: no se encontró el evento '{label}'.");
            return;
        }
        TryFire(entry);
    }

    /// <summary>
    /// Resetea el estado de un evento por su label, permitiendo que vuelva a dispararse aunque sea oneShot.
    /// </summary>
    public void ResetEventByLabel(string label)
    {
        var entry = events.Find(e => e.label == label);
        if (entry == null) return;
        entry.hasTriggered = false;
        entry.lastTriggeredTime = -999f;
    }
}
