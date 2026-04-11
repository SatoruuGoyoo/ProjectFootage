using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct RecordingData
{
    public List<Texture2D> frames;
    public AudioClip clip;
}

public class CamcorderStorage : MonoBehaviour
{
    private List<RecordingData> recordings = new List<RecordingData>();
    private const int maxRecordings = 5;

    public void AddRecording(List<Texture2D> frames, AudioClip clip = null)
    {
        if (recordings.Count >= maxRecordings)
            recordings.RemoveAt(0);

        recordings.Add(new RecordingData { frames = frames, clip = clip });
    }

    public List<RecordingData> GetAllRecordings() => new List<RecordingData>(recordings);

    public void DiscardRecording(int index)
    {
        if (index >= 0 && index < recordings.Count)
            recordings.RemoveAt(index);
    }
}