using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Reproduce todos los RecordedAudioTrack de la sesión
/// en sincronía con el PlaybackClock.
/// </summary>
[RequireComponent(typeof(PlaybackClock))]
public class AudioPlayback : MonoBehaviour
{
    [Header("Listener 3D")]
    public Transform playbackListener;

    private PlaybackClock _clock;
    private RecordingSession _session;

    // Una instancia FMOD por track grabado
    private readonly List<EventInstance> _instances = new List<EventInstance>();

    private void Awake() => _clock = GetComponent<PlaybackClock>();

    private void OnEnable()
    {
        _clock.OnPlay += OnPlay;
        _clock.OnPause += OnPause;
        _clock.OnStop += OnStop;
        _clock.OnSeek += OnSeek;
        _clock.OnComplete += OnStop;
    }

    private void OnDisable()
    {
        _clock.OnPlay -= OnPlay;
        _clock.OnPause -= OnPause;
        _clock.OnStop -= OnStop;
        _clock.OnSeek -= OnSeek;
        _clock.OnComplete -= OnStop;
    }

    public void Load(RecordingSession session)
    {
        _session = session;
    }

    // ── Respuestas al Clock ────────────────────────────────────

    private void OnPlay()
    {
        if (_session == null) return;
        CreateInstances();
        UpdateListenerPosition(_clock.CurrentTime);
        StartAllAt(_clock.CurrentTime);
    }

    private void OnPause()
    {
        foreach (var inst in _instances)
            inst.setPaused(true);
    }

    private void OnStop()
    {
        StopAndReleaseAll();
        _session = null;
    }

    private void OnSeek(float time)
    {
        int ms = Mathf.RoundToInt(time * 1000f);
        foreach (var inst in _instances)
            inst.setTimelinePosition(ms);
        UpdateListenerPosition(time);
    }

    private void Update()
    {
        if (!_clock.IsPlaying || _instances.Count == 0) return;
        UpdateListenerPosition(_clock.CurrentTime);
    }

    // ── Helpers ────────────────────────────────────────────────

    private void CreateInstances()
    {
        StopAndReleaseAll();

        foreach (var track in _session.AudioTracks)
        {
            if (string.IsNullOrEmpty(track.FMODPath)) continue;

            var inst = RuntimeManager.CreateInstance(track.FMODPath);

            // Si el evento es 3D, lo attachamos a la posición grabada
            if (track.Is3D && playbackListener != null)
                RuntimeManager.AttachInstanceToGameObject(inst, playbackListener);

            _instances.Add(inst);
            Debug.Log($"AudioPlayback: instancia creada para '{track.FMODPath}'");
        }
    }

    private void StartAllAt(float time)
    {
        int ms = Mathf.RoundToInt(time * 1000f);
        for (int i = 0; i < _instances.Count; i++)
        {
            var inst = _instances[i];
            var track = _session.AudioTracks[i];

            // Calculamos dónde estaba el evento cuando se grabó
            int seekMs = track.FMODTimelinePosition + ms;
            inst.setTimelinePosition(seekMs);
            inst.setPaused(false);
            inst.start();
        }
    }

    private void StopAndReleaseAll()
    {
        foreach (var inst in _instances)
        {
            inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            inst.release();
        }
        _instances.Clear();
    }

    private void UpdateListenerPosition(float time)
    {
        if (_session == null || playbackListener == null) return;
        CameraTransformFrame? cam = _session.GetCameraAtTime(time);
        if (cam == null) return;
        playbackListener.position = cam.Value.Position;
        playbackListener.rotation = cam.Value.Rotation;
    }

    private void OnDestroy() => StopAndReleaseAll();
}