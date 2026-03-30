using System.Collections.Generic;
using UnityEngine;

public class CamcorderStorage : MonoBehaviour
{
    private List<List<Texture2D>> recordings = new List<List<Texture2D>>();
    private const int maxRecordings = 5;

    public void AddRecording(List<Texture2D> frames)
    {
        if (recordings.Count >= maxRecordings)
            recordings.RemoveAt(0); // Remove the oldest recording if we exceed the limit

        recordings.Add(frames);
    }

    public List<List<Texture2D>> GetAllRecordings()
    {
        return new List<List<Texture2D>>(recordings);
    }

    public void DiscardRecording(int index)
    {
        if (index >= 0 && index < recordings.Count)
            recordings.RemoveAt(index);
    }

}
