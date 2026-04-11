using System.Collections.Generic;
using UnityEngine;

public class CamcorderRecorder : MonoBehaviour
{
    [Header("Recording")]
    public RenderTexture recordingTexture;

    [Header("Diegetic Audio")]
    public Transform camViewpoint;
    public CamcorderDiegeticAudio[] diegeticAudioSources;

    public List<Texture2D> framesRecorded = new List<Texture2D>();
    public List<Texture2D> GetRecording() => new List<Texture2D>(framesRecorded);
    public int DiegeticSourceCount => diegeticAudioSources != null ? diegeticAudioSources.Length : 0;
    public bool IsRecording { get; private set; } = false;

    private List<List<float>> audioSamples = new List<List<float>>();
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

    public void StartRecording()
    {
        framesRecorded.Clear();
        audioSamples.Clear();

        if (diegeticAudioSources != null)
            foreach (var _ in diegeticAudioSources)
                audioSamples.Add(new List<float>());

        captureTimer = 0f;
        IsRecording = true;
        GameEvents.RecordingStarted();
    }

    public void StopRecording()
    {
        IsRecording = false;
        captureTimer = 0f;
        GameEvents.RecordingStopped();
    }

    public List<float> GetAudioSamples(int sourceIndex)
    {
        if (audioSamples == null || sourceIndex >= audioSamples.Count) return null;
        return new List<float>(audioSamples[sourceIndex]);
    }

    private void CaptureFrame()
    {
        RenderTexture.active = recordingTexture;
        Texture2D frame = new Texture2D(recordingTexture.width, recordingTexture.height, TextureFormat.RGB24, false);
        frame.ReadPixels(new Rect(0, 0, recordingTexture.width, recordingTexture.height), 0, 0);
        frame.Apply();
        RenderTexture.active = null;
        framesRecorded.Add(frame);

        if (diegeticAudioSources == null || camViewpoint == null) return;

        Vector3 camPos = camViewpoint.position;
        Vector3 camFwd = camViewpoint.forward;

        for (int i = 0; i < diegeticAudioSources.Length; i++)
        {
            if (diegeticAudioSources[i] == null) continue;
            audioSamples[i].Add(diegeticAudioSources[i].SampleVolume(camPos, camFwd));
        }
    }
}