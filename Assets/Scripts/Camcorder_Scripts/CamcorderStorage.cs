using System.Collections.Generic;
using UnityEngine;

public class CamcorderStorage : MonoBehaviour
{
    private Queue<List<Texture2D>> recordingQueue = new Queue<List<Texture2D>>();
    private const int maxRecordings = 5;

    public void AddRecording(List<Texture2D> frames)
    {
        if(recordingQueue.Count >= maxRecordings)
        {
            recordingQueue.Dequeue();
        }

        recordingQueue.Enqueue(frames);
    }

    public List<List<Texture2D>> GetAllRecordings()
    {
        return new List<List<Texture2D>>(recordingQueue);
    }

}
