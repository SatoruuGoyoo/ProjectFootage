using FMODUnity;
using UnityEngine;

/// <summary>
/// Responsabilidad única: registrar qué evento FMOD estaba sonando
/// y en qué momento de la grabación arrancó.
/// No captura samples de audio — FMOD maneja su propio timeline.
/// Lo que guardamos es suficiente para que AudioPlayback
/// arranque el mismo evento desde el mismo punto.
/// </summary>
public class AudioRecorder : MonoBehaviour
{
    // ── API pública ────────────────────────────────────────────

    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _recordingTimer;

    public void StartRecording(RecordingSession session, EventReference fmodEvent)
    {
        if (IsRecording) return;

        _session = session;
        _recordingTimer = 0f;
        IsRecording = true;

        //// Guardamos un registro de la pista de audio que estaba sonando
        //// usando el nuevo API: RecordingSession.RegisterAudioTrack.
        //// Rellenamos los campos mínimos (path/tiempo) — otros valores
        //// pueden ajustarse más adelante si se requiere mayor precisión.
        var track = new RecordedAudioTrack
        {
            FMODPath = fmodEvent.Guid.ToString(),
            StartTime = 0f,
            FMODTimelinePosition = 0,
            Is3D = false,
            Position = Vector3.zero
        };
        _session.RegisterAudioTrack(track);
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        _session = null;
    }

    private void Update()
    {
        if (!IsRecording) return;
        _recordingTimer += Time.deltaTime;
    }
}