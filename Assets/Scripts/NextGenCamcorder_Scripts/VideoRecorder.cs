using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class VideoRecorder : MonoBehaviour
{
    [Header("Setup")]
    public RenderTexture recordingTexture;

    [Header("Camera — para audio 3D posicional")]
    public Transform cameraTransform;

    [Header("Tweaks")]
    [SerializeField] private float captureInterval = 0.125f;  // 8fps

    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _captureTimer;
    private float _recordingTimer;

    public void StartRecording(RecordingSession session)
    {
        if (IsRecording) return;

        _session = session;
        _captureTimer = 0f;
        _recordingTimer = 0f;
        IsRecording = true;
    }

    public void StopRecording()
    {
        if (!IsRecording) return;
        IsRecording = false;
        Debug.Log("VideoRecorder grabación terminada {_session.VideoFrames.Count} frames capturados");
    }

    private void Update()
    {
        if (!IsRecording) return;

        _recordingTimer += Time.deltaTime;
        _captureTimer += Time.deltaTime;

        if (_captureTimer >= captureInterval)
        {
            _captureTimer = 0f;
            CaptureTransform(_recordingTimer); 
            RequestFrame(_recordingTimer);      
        }
    }

    private void CaptureTransform(float timestamp)
    {
        if (cameraTransform == null) return;

        _session.AddCameraFrame(new CameraTransformFrame(
            cameraTransform.position,
            cameraTransform.rotation,
            timestamp
        ));
    }

    private void RequestFrame(float timestamp)
    {
        AsyncGPUReadback.Request(recordingTexture, 0, TextureFormat.RGB24,
            req => OnFrameReceived(req, timestamp));
    }

    private void OnFrameReceived(AsyncGPUReadbackRequest req, float timestamp)
    {
        if (req.hasError || _session == null) return;

        NativeArray<byte> raw = req.GetData<byte>();
        byte[] pixels = new byte[raw.Length];
        raw.CopyTo(pixels);

        _session.AddVideoFrame(new VideoFrame(pixels, timestamp));
    }
}