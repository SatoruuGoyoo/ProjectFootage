using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpatialAudioRecorder : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("El transform de la cámara de la camcorder — el mismo que usa VideoRecorder")]
    public Transform cameraTransform;

    [Header("Config")]
    [Tooltip("Debe coincidir con captureInterval de VideoRecorder para que los keyframes queden sincronizados con los frames de video")]
    [SerializeField] private float captureInterval = 0.125f;

    [Tooltip("Cuánto influye el ángulo de la cámara en el volumen grabado.\n0 = solo distancia\n1 = distancia × ángulo completo\n0.6 = similar a micrófono cardioide")]
    [Range(0f, 1f)]
    [SerializeField] private float angleInfluence = 0.6f;

    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _captureTimer;
    private float _recordingTimer;

    // Guardamos referencia directa a la lista de keyframes de cada fuente.
    // Así podemos escribir en ella aunque el struct RecordedAudioTrack ya fue copiado
    // dentro de RecordingSession.
    private readonly List<(ISpatialAudioSource source, List<AudioVolumeKeyFrame> keyframes)> _registered
        = new List<(ISpatialAudioSource, List<AudioVolumeKeyFrame>)>();

    // ── API pública ────────────────────────────────────────────

    public void StartRecording(RecordingSession session)
    {
        if (IsRecording) return;
        _session = session;
        _captureTimer = 0f;
        _recordingTimer = 0f;
        IsRecording = true;
        RegisterAllSources();
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        _registered.Clear();
        _session = null;
    }

    // ── Registro inicial ───────────────────────────────────────

    private void RegisterAllSources()
    {
        _registered.Clear();

        // Encuentra todo MonoBehaviour en la escena que implemente ISpatialAudioSource.
        // No hay acoplamiento — no sabe si es una tele, una cuchara o lo que sea.
        var sources = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISpatialAudioSource>();

        foreach (var source in sources)
        {
            if (!source.IsActiveInScene) continue;

            source.TryGetTimelinePosition(out int timelinePos);

            // Creamos la lista ANTES de construir el struct para retener la referencia
            var keyframes = new List<AudioVolumeKeyFrame>();

            var track = new RecordedAudioTrack
            {
                FMODPath = source.FMODPath,
                StartTime = 0f,
                FMODTimelinePosition = timelinePos,
                Is3D = source.Is3D,
                Position = source.WorldPosition,
                VolumeKeyframes = keyframes
            };

            _session.RegisterAudioTrack(track);
            _registered.Add((source, keyframes));
        }

        Debug.Log($"SpatialAudioRecorder: {_registered.Count} fuentes registradas");
    }

    // ── Loop ──────────────────────────────────────────────────

    private void Update()
    {
        if (!IsRecording) return;
        _recordingTimer += Time.deltaTime;
        _captureTimer += Time.deltaTime;

        if (_captureTimer >= captureInterval)
        {
            _captureTimer = 0f;
            BakeKeyframes(_recordingTimer);
        }
    }

    private void BakeKeyframes(float timestamp)
    {
        if (cameraTransform == null) return;

        foreach (var (source, keyframes) in _registered)
        {
            float vol = SpatialAudioEvaluator.ComputeVolume(
                cameraTransform.position,
                cameraTransform.rotation,
                source,
                angleInfluence
            );

            keyframes.Add(new AudioVolumeKeyFrame
            {
                Timestamp = timestamp,
                Volume = vol
            });
        }
    }
}