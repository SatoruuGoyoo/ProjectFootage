using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsabilidad única: guardar y eliminar RecordingSessions.
/// Reemplaza el RecordingData viejo (List<Texture2D> + AudioClip)
/// por RecordingSession que tiene todo — video, audio, transforms.
/// La interfaz pública es casi idéntica a la anterior.
/// </summary>
public class CamcorderStorage : MonoBehaviour
{
    private readonly List<RecordingSession> _recordings = new List<RecordingSession>();
    private const int MaxRecordings = 5;

    public int Count => _recordings.Count;

    public void AddRecording(RecordingSession session)
    {
        if (!session.IsCompleted)
        {
            Debug.LogWarning("CamcorderStorage: se intentó guardar una sesión no completada.");
            return;
        }

        if (_recordings.Count >= MaxRecordings)
            _recordings.RemoveAt(0);

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