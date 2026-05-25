using System.Collections.Generic;
using UnityEngine;

public class CamcorderStorage : MonoBehaviour
{
    private readonly List<RecordingSession> _recordings = new List<RecordingSession>();
    private const int MaxRecordings = 5;

    public int Count => _recordings.Count;
    public bool IsFull => _recordings.Count >= MaxRecordings;

    public void AddRecording(RecordingSession session)
    {
        if (!session.IsCompleted) return;
        if (IsFull) return;

        _recordings.Add(session);
    }

    public IReadOnlyList<RecordingSession> GetAllRecordings() => _recordings;

    public RecordingSession GetRecording(int index)
    {
        if (index < 0 || index >= _recordings.Count) return null;
        return _recordings[index];
    }

    public void DiscardRecording(int index)
    {
        if (index >= 0 && index < _recordings.Count)
            _recordings.RemoveAt(index);
    }
}