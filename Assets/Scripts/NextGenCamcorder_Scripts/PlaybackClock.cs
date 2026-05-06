using System;
using UnityEngine;

/// <summary>
/// Responsabilidad única: manejar el tiempo de reproducción.
/// Es el único que sabe "en qué segundo estamos" durante el playback.
/// VideoPlayback y AudioPlayback no tienen timers propios —
/// todos preguntan acá y así nunca se dessincronizan.
/// </summary>
public class PlaybackClock : MonoBehaviour
{
    // ── Estado ────────────────────────────────────────────────
    public float CurrentTime { get; private set; }
    public float Duration { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished => CurrentTime >= Duration && Duration > 0f;
    public bool HasSession => _session != null;

    // ── Eventos — cualquier sistema se suscribe sin acoplarse ──
    public event Action OnPlay;
    public event Action OnPause;
    public event Action OnStop;
    public event Action<float> OnSeek;      // float = nuevo tiempo
    public event Action OnComplete;  // llegó al final

    private RecordingSession _session;

    // ── API pública ────────────────────────────────────────────

    public void Load(RecordingSession session)
    {
        Stop();
        _session = session;
        Duration = session.Duration;
        CurrentTime = 0f;
    }

    public void Play()
    {
        if (_session == null || IsFinished) return;
        IsPlaying = true;
        OnPlay?.Invoke();
    }

    public void Pause()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        OnPause?.Invoke();
    }

    public void Stop()
    {
        IsPlaying = false;
        CurrentTime = 0f;
        _session = null;
        OnStop?.Invoke();
    }

    /// <summary>
    /// Salta a un tiempo exacto. Usado por Rewind/FastForward.
    /// Todos los sistemas reciben OnSeek y se reposicionan solos.
    /// </summary>
    public void Seek(float time)
    {
        CurrentTime = Mathf.Clamp(time, 0f, Duration);
        OnSeek?.Invoke(CurrentTime);
    }

    /// <summary>
    /// Avanza o retrocede N segundos desde la posición actual.
    /// Usado por los botones de RFF del menu.
    /// </summary>
    public void SeekDelta(float deltaSeconds)
    {
        Seek(CurrentTime + deltaSeconds);
    }

    // ── Loop ──────────────────────────────────────────────────

    private void Update()
    {
        if (!IsPlaying) return;

        CurrentTime += Time.deltaTime;

        if (CurrentTime >= Duration)
        {
            CurrentTime = Duration;
            IsPlaying = false;
            OnComplete?.Invoke();
        }
    }
}