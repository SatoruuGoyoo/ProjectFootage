using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Responsabilidad única: reproducir el audio grabado en sincronía
/// con el PlaybackClock, y mover el listener de FMOD a la posición
/// que tenía la cámara cuando grabó — para audio 3D posicional correcto.
/// No sabe nada de video ni de UI.
/// </summary>
[RequireComponent(typeof(PlaybackClock))]
public class AudioPlayback : MonoBehaviour
{
    [Header("Listener 3D")]
    [Tooltip("Transform que representa el listener durante el playback. " +
             "Se mueve a la posición grabada de la cámara en cada frame.")]
    public Transform playbackListener;

    private PlaybackClock _clock;
    private RecordingSession _session;
    private EventInstance _eventInstance;
    private bool _hasInstance;

    private void Awake()
    {
        _clock = GetComponent<PlaybackClock>();
    }

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

    // ── API pública ────────────────────────────────────────────

    public void Load(RecordingSession session)
    {
        _session = session;
    }

    // ── Respuestas al Clock ────────────────────────────────────

    private void OnPlay()
    {
        if (_session == null || string.IsNullOrEmpty(_session.FMODAudioPath)) return;

        if (!_hasInstance)
        {
            _eventInstance = RuntimeManager.CreateInstance(_session.FMODAudioPath);
            _hasInstance = true;
        }

        // Posicionamos el listener antes de arrancar
        UpdateListenerPosition(_clock.CurrentTime);

        // Seek al tiempo correcto — por si arranca desde pausa o desde medio
        int ms = Mathf.RoundToInt(_clock.CurrentTime * 1000f);
        _eventInstance.setTimelinePosition(ms);
        _eventInstance.setPaused(false);
        _eventInstance.start();
    }

    private void OnPause()
    {
        if (!_hasInstance) return;
        _eventInstance.setPaused(true);
    }

    private void OnStop()
    {
        if (!_hasInstance) return;
        _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _eventInstance.release();
        _hasInstance = false;
        _session = null;
    }

    private void OnSeek(float time)
    {
        if (!_hasInstance) return;

        // RFF — hacemos seek en FMOD al mismo tiempo que el clock
        int ms = Mathf.RoundToInt(time * 1000f);
        _eventInstance.setTimelinePosition(ms);
        UpdateListenerPosition(time);
    }

    // ── Loop ──────────────────────────────────────────────────

    private void Update()
    {
        if (!_clock.IsPlaying || !_hasInstance) return;

        // Actualizamos la posición del listener cada frame
        // para que el audio 3D sea correcto durante la reproducción
        UpdateListenerPosition(_clock.CurrentTime);
    }

    // ── Audio 3D posicional ───────────────────────────────────

    private void UpdateListenerPosition(float time)
    {
        if (_session == null || playbackListener == null) return;

        CameraTransformFrame? camFrame = _session.GetCameraAtTime(time);
        if (camFrame == null) return;

        // Movemos el listener a donde estaba la cámara cuando grabó
        // FMOD calcula el audio 3D desde esta posición automáticamente
        playbackListener.position = camFrame.Value.Position;
        playbackListener.rotation = camFrame.Value.Rotation;
    }

    private void OnDestroy()
    {
        if (_hasInstance)
        {
            _eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _eventInstance.release();
        }
    }
}