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

        // Guardamos el path del evento FMOD y en qué timestamp arrancó
        // (casi siempre 0, pero si en el futuro querés grabar
        // desde la mitad de un evento, ya está contemplado)
        _session.SetAudioData(fmodEvent.Path, 0f);
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