using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(PlaybackClock))]
public class AudioPlayback : MonoBehaviour
{
    [Header("Listener 3D")]
    public Transform playbackListener;

    private PlaybackClock _clock;
    private RecordingSession _session;
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

    public void Load(RecordingSession session) => _session = session;

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
            if (inst.isValid()) inst.setPaused(true);
    }

    private void OnStop()
    {
        StopAndReleaseAll();
        _session = null;
    }

    private void OnSeek(float time)
    {
        int ms = Mathf.RoundToInt(time * 1000f);
        for (int i = 0; i < _instances.Count; i++)
        {
            if (!_instances[i].isValid()) continue;
            _instances[i].setTimelinePosition(ms);

            // Aplicamos el volumen inmediatamente en el seek para que no haya flash de volumen
            var track = _session.AudioTracks[i];
            if (track.VolumeKeyframes != null && track.VolumeKeyframes.Count > 0)
                _instances[i].setVolume(SampleVolumeAt(track.VolumeKeyframes, time));
        }
        UpdateListenerPosition(time);
    }

    private void Update()
    {
        if (!_clock.IsPlaying || _instances.Count == 0) return;

        float currentTime = _clock.CurrentTime;

        for (int i = 0; i < _instances.Count; i++)
        {
            if (!_instances[i].isValid()) continue;

            var track = _session.AudioTracks[i];
            if (track.VolumeKeyframes != null && track.VolumeKeyframes.Count > 0)
            {
                float vol = SampleVolumeAt(track.VolumeKeyframes, currentTime);
                _instances[i].setVolume(vol);
            }
        }

        UpdateListenerPosition(currentTime);
    }

    // ── Helpers ────────────────────────────────────────────────

    private void CreateInstances()
    {
        StopAndReleaseAll();

        foreach (var track in _session.AudioTracks)
        {
            if (string.IsNullOrEmpty(track.FMODPath))
            {
                _instances.Add(new EventInstance()); // placeholder inválido
                continue;
            }

            // Si la fuente nunca fue audible, no creamos instancia FMOD
            if (IsTrackSilent(track))
            {
                _instances.Add(new EventInstance());
                continue;
            }

            var inst = RuntimeManager.CreateInstance(track.FMODPath);

            // Solo usamos el sistema 3D de FMOD si la fuente NO tiene keyframes.
            // Con keyframes el volumen está bakeado — attachar la fuente al listener
            // causaría que FMOD la atenuara dos veces.
            bool hasKeyframes = track.VolumeKeyframes != null && track.VolumeKeyframes.Count > 0;
            if (track.Is3D && playbackListener != null && !hasKeyframes)
                RuntimeManager.AttachInstanceToGameObject(inst, playbackListener);

            _instances.Add(inst);
        }
    }

    private void StartAllAt(float time)
    {
        int ms = Mathf.RoundToInt(time * 1000f);
        for (int i = 0; i < _instances.Count; i++)
        {
            if (!_instances[i].isValid()) continue;

            var track = _session.AudioTracks[i];
            int seekMs = track.FMODTimelinePosition + ms;
            _instances[i].setTimelinePosition(seekMs);
            _instances[i].setPaused(false);
            _instances[i].start();
        }
    }

    private void StopAndReleaseAll()
    {
        foreach (var inst in _instances)
        {
            if (!inst.isValid()) continue;
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

    /// <summary>
    /// Interpolación lineal entre los dos keyframes más cercanos al tiempo dado.
    /// Usa binary search — O(log n).
    /// </summary>
    private static float SampleVolumeAt(List<AudioVolumeKeyFrame> keyframes, float time)
    {
        if (keyframes.Count == 0) return 1f;
        if (keyframes.Count == 1) return keyframes[0].Volume;

        if (time <= keyframes[0].Timestamp) return keyframes[0].Volume;
        if (time >= keyframes[keyframes.Count - 1].Timestamp) return keyframes[keyframes.Count - 1].Volume;

        int lo = 0, hi = keyframes.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi + 1) / 2;
            if (keyframes[mid].Timestamp <= time) lo = mid;
            else hi = mid - 1;
        }

        var a = keyframes[lo];
        var b = keyframes[lo + 1];
        float t = Mathf.InverseLerp(a.Timestamp, b.Timestamp, time);
        return Mathf.Lerp(a.Volume, b.Volume, t);
    }

    /// <summary>
    /// Devuelve true si todos los keyframes tienen volumen menor a 0.01.
    /// Sirve para evitar crear instancias FMOD de fuentes que nunca fueron apuntadas.
    /// </summary>
    private static bool IsTrackSilent(RecordedAudioTrack track)
    {
        if (track.VolumeKeyframes == null || track.VolumeKeyframes.Count == 0) return false;
        foreach (var kf in track.VolumeKeyframes)
            if (kf.Volume > 0.01f) return false;
        return true;
    }

    private void OnDestroy() => StopAndReleaseAll();
}