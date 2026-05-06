using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Responsabilidad única: capturar frames de video Y el transform
/// de la cámara en cada frame, y escribirlos en la RecordingSession.
/// El transform se usa en playback para posicionar el listener de FMOD
/// y reproducir el audio 3D igual a como lo escuchó el jugador.
/// </summary>
public class VideoRecorder : MonoBehaviour
{
    [Header("Setup")]
    public RenderTexture recordingTexture;

    [Header("Camera — para audio 3D posicional")]
    [Tooltip("La cámara de la camcorder. Se graba su posición y rotación en cada frame.")]
    public Transform cameraTransform;

    [Header("Tweaks")]
    [SerializeField] private float captureInterval = 0.125f;  // 8fps

    public bool IsRecording { get; private set; }

    private RecordingSession _session;
    private float _captureTimer;
    private float _recordingTimer;

    // ── API pública ────────────────────────────────────────────

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
    }

    // ── Loop ──────────────────────────────────────────────────

    private void Update()
    {
        if (!IsRecording) return;

        _recordingTimer += Time.deltaTime;
        _captureTimer += Time.deltaTime;

        if (_captureTimer >= captureInterval)
        {
            _captureTimer = 0f;
            CaptureTransform(_recordingTimer);  // síncrono — barato
            RequestFrame(_recordingTimer);       // asíncrono — pesado, va a la GPU
        }
    }

    // ── Captura transform (síncrono, sin costo) ───────────────

    private void CaptureTransform(float timestamp)
    {
        if (cameraTransform == null) return;

        _session.AddCameraFrame(new CameraTransformFrame(
            cameraTransform.position,
            cameraTransform.rotation,
            timestamp
        ));
    }

    // ── Captura video (asíncrono, GPU) ────────────────────────

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