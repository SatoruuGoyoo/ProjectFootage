using System.Collections.Generic;
using UnityEngine;

public class CamcorderRecorder : MonoBehaviour
{
    public RenderTexture recordingTexture;
    public List<Texture2D> framesRecorded = new List<Texture2D>();
    public List<Texture2D> GetRecording() => new List<Texture2D>(framesRecorded);

    public bool IsRecording { get; private set; } = false;

    private float captureTimer = 0f;
    [SerializeField] private float captureInterval = 0.125f;

    private void Update()
    {
        if (!IsRecording) return;

        captureTimer += Time.deltaTime;
        if (captureTimer >= captureInterval)
        {
            captureTimer = 0f;
            CaptureFrame();
        }
    }

    // Captures the current frame from the RenderTexture and stores it in the framesRecorded list
    private void CaptureFrame()
    {
        RenderTexture.active = recordingTexture;
        Texture2D frame = new Texture2D(recordingTexture.width, recordingTexture.height, TextureFormat.RGB24, false);
        frame.ReadPixels(new Rect(0, 0, recordingTexture.width, recordingTexture.height), 0, 0);
        frame.Apply();
        RenderTexture.active = null;
        framesRecorded.Add(frame);
    }

    // Starts the recording process by clearing previous frames and resetting the timer
    public void StartRecording()
    {
        framesRecorded.Clear();
        captureTimer = 0f;
        IsRecording = true;
    }

    // Stops the recording process and resets the timer
    public void StopRecording()
    {
        IsRecording = false;
        captureTimer = 0f;
    }
}