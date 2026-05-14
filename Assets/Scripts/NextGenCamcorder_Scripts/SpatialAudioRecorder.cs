using System.Collections.Generic;
using System.Linq;
using FMODUnity;
using UnityEngine;

public class SpatialAudioRecorder : MonoBehaviour
{
    [Header("Setup")]
    public Transform cameraTransform;

    [Header("Config")]
    [SerializeField] private float captureInterval = 0.125f;

    [Range(0f, 1f)]
    [SerializeField] private float angleInfluence = 0.6f;

    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _captureTimer;
    private float _recordingTimer;

    private readonly List<(ISpatialAudioSource source, List<AudioVolumeKeyFrame> keyframes)> _registered
        = new List<(ISpatialAudioSource, List<AudioVolumeKeyFrame>)>();

    // ── Static accessor para one-shots desde cualquier script ──

    private static SpatialAudioRecorder _activeRecorder;

    private void OnEnable() => _activeRecorder = this;
    private void OnDisable() { if (_activeRecorder == this) _activeRecorder = null; }

    /// <summary>
    /// Llamable desde cualquier script. Si hay grabación activa, registra un one-shot
    /// con el volumen calculado al momento del trigger. Si no, no pasa nada.
    /// </summary>
    public static bool TryCaptureOneShot(EventReference fmodEvent, Vector3 position, float maxAudibleDistance = 20f)
    {
        if (_activeRecorder == null || !_activeRecorder.IsRecording) return false;
        if (fmodEvent.IsNull) return false;
        return _activeRecorder.CaptureOneShotInternal(fmodEvent, position, maxAudibleDistance);
    }

    // ── API de grabación ───────────────────────────────────────

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

    // ── Registro inicial de fuentes continuas ──────────────────

    private void RegisterAllSources()
    {
        _registered.Clear();

        var sources = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<ISpatialAudioSource>();

        foreach (var source in sources)
        {
            if (!source.IsActiveInScene) continue;

            source.TryGetTimelinePosition(out int timelinePos);

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

    // ── Captura de one-shot ────────────────────────────────────

    private bool CaptureOneShotInternal(EventReference fmodEvent, Vector3 position, float maxAudibleDistance)
    {
        if (cameraTransform == null) return false;

        float vol = SpatialAudioEvaluator.ComputeVolume(
            cameraTransform.position,
            cameraTransform.rotation,
            position,
            maxAudibleDistance,
            angleInfluence
        );
        string fmodGuid = fmodEvent.Guid.ToString();
        _session.RegisterOneShot(new RecordedOneShotEvent
        {
            FMODPath = fmodGuid,
            Timestamp = _recordingTimer,
            Position = position,
            Volume = vol
        });

        Debug.Log($"... one-shot '{fmodGuid}' capturado en t={_recordingTimer:F2}s vol={vol:F2}");
        return true;
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