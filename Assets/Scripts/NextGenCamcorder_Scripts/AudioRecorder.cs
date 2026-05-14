using FMODUnity;
using UnityEngine;

public class AudioRecorder : MonoBehaviour
{
    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _recordingTimer;

    public void StartRecording(RecordingSession session, EventReference fmodEvent)
    {
        if (IsRecording) return;

        _session = session;
        _recordingTimer = 0f;
        IsRecording = true;

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