using System;
using UnityEngine;

public class PlaybackClock : MonoBehaviour
{
    public float CurrentTime { get; private set; }
    public float Duration { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsFinished => CurrentTime >= Duration && Duration > 0f;
    public bool HasSession => _session != null;
    public RecordingSession CurrentSession => _session;

    public event Action OnPlay;
    public event Action OnPause;
    public event Action OnStop;
    public event Action<float> OnSeek;     
    public event Action OnComplete; 

    private RecordingSession _session;

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

    public void Seek(float time)
    {
        CurrentTime = Mathf.Clamp(time, 0f, Duration);
        OnSeek?.Invoke(CurrentTime);
    }

    public void SeekDelta(float deltaSeconds)
    {
        Seek(CurrentTime + deltaSeconds);
    }

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