using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Ponelo en cualquier GameObject que tenga un evento FMOD
/// que quieras que se escuche en el footage grabado.
/// Se registra solo en la RecordingSession cuando empieza la grabación.
/// No sabe nada de VideoRecorder ni de PlaybackClock.
/// </summary>
public class RecordableAudioSource : MonoBehaviour
{
    [Header("FMOD")]
    public EventReference audioEvent;

    [Header("Config")]
    [Tooltip("Si es 3D, el audio se escucha desde la posición de este GameObject.")]
    public bool is3D = false;

    private EventInstance _instance;
    private bool _hasInstance;

    private void OnEnable() => GameEvents.OnRecordingStarted += OnRecordingStarted;
    private void OnDisable() => GameEvents.OnRecordingStarted -= OnRecordingStarted;

    /// <summary>
    /// Cuando arranca la grabación, nos registramos en la sesión.
    /// CamcorderController dispara GameEvents.RecordingStarted(session).
    /// </summary>
    private void OnRecordingStarted(RecordingSession session)
    {
        if (audioEvent.IsNull) return;

        // Obtenemos el timeline position actual del evento si ya estaba corriendo
        int timelinePos = 0;
        if (_hasInstance)
            _instance.getTimelinePosition(out timelinePos);

        session.RegisterAudioTrack(new RecordedAudioTrack
        {
            FMODPath = audioEvent.Path,
            StartTime = 0f,
            FMODTimelinePosition = timelinePos,
            Is3D = is3D,
            Position = transform.position
        });

        Debug.Log($"RecordableAudioSource: registrado '{audioEvent.Path}' en la sesión");
    }

    // ── API para que el manager arranque/pare el evento ────────
    // (HallwayAudioManager e IterationPlaybackAudio siguen manejando
    //  cuándo suena el evento en el mundo — esto solo lo registra)

    public void SetInstance(EventInstance instance)
    {
        _instance = instance;
        _hasInstance = true;
    }
}