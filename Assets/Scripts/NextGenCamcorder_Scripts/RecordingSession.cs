using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objeto de datos puro que representa una grabación completa.
/// No es MonoBehaviour — no vive en la escena, solo en memoria.
/// </summary>
public class RecordingSession
{
    // ── Metadata ──────────────────────────────────────────────
    public float Duration { get; private set; }
    public bool IsCompleted { get; private set; }

    // ── Video ─────────────────────────────────────────────────
    private readonly List<VideoFrame> _videoFrames = new List<VideoFrame>();
    public IReadOnlyList<VideoFrame> VideoFrames => _videoFrames;

    // ── Camera Transform ──────────────────────────────────────
    private readonly List<CameraTransformFrame> _cameraFrames = new List<CameraTransformFrame>();
    public IReadOnlyList<CameraTransformFrame> CameraFrames => _cameraFrames;

    // ── Audio ─────────────────────────────────────────────────
    public string FMODAudioPath { get; private set; }
    public float FMODStartTime { get; private set; }

    // ── Escritura (solo Recorders) ─────────────────────────────

    public void AddVideoFrame(VideoFrame frame)
    {
        if (IsCompleted) return;
        if (frame.Timestamp < 0) throw new ArgumentException("Timestamp negativo");
        _videoFrames.Add(frame);
    }

    public void AddCameraFrame(CameraTransformFrame frame)
    {
        if (IsCompleted) return;
        if (frame.Timestamp < 0) throw new ArgumentException("Timestamp negativo");
        _cameraFrames.Add(frame);
    }

    public void SetAudioData(string fmodPath, float startTime)
    {
        if (IsCompleted) return;
        FMODAudioPath = fmodPath;
        FMODStartTime = startTime;
    }

    public void Complete(float duration)
    {
        if (duration < 0) throw new ArgumentException("Duración negativa");
        Duration = duration;
        IsCompleted = true;
    }

    // ── Lectura (Playback systems) ─────────────────────────────

    /// <summary>
    /// Dado un tiempo de reproducción, devuelve el VideoFrame más cercano.
    /// </summary>
    public VideoFrame? GetFrameAtTime(float time)
    {
        if (_videoFrames.Count == 0) return null;
        return BinarySearch(_videoFrames, f => f.Timestamp, time);
    }

    /// <summary>
    /// Dado un tiempo de reproducción, devuelve el CameraTransformFrame más cercano.
    /// AudioPlayback usa esto para posicionar el listener de FMOD.
    /// </summary>
    public CameraTransformFrame? GetCameraAtTime(float time)
    {
        if (_cameraFrames.Count == 0) return null;
        return BinarySearch(_cameraFrames, f => f.Timestamp, time);
    }

    // ── Búsqueda binaria genérica ──────────────────────────────
    private static T? BinarySearch<T>(List<T> list, Func<T, float> getTimestamp, float time)
        where T : struct
    {
        if (list.Count == 0) return null;
        int lo = 0, hi = list.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi + 1) / 2;
            if (getTimestamp(list[mid]) <= time) lo = mid;
            else hi = mid - 1;
        }
        return list[lo];
    }
}

/// <summary>
/// Frame de video: pixels crudos en RAM + timestamp.
/// </summary>
public readonly struct VideoFrame
{
    public readonly byte[] PixelData;  // RGB24, 640x480
    public readonly float Timestamp;

    public VideoFrame(byte[] pixelData, float timestamp)
    {
        if (timestamp < 0) throw new ArgumentException("Timestamp negativo");
        PixelData = pixelData;
        Timestamp = timestamp;
    }
}

/// <summary>
/// Posición y rotación de la cámara en un momento de la grabación.
/// AudioPlayback mueve el listener de FMOD a esta posición durante el playback
/// para que el audio 3D sea idéntico a lo que escuchó el jugador al grabar.
/// </summary>
public readonly struct CameraTransformFrame
{
    public readonly Vector3 Position;
    public readonly Quaternion Rotation;
    public readonly float Timestamp;

    public CameraTransformFrame(Vector3 position, Quaternion rotation, float timestamp)
    {
        if (timestamp < 0) throw new ArgumentException("Timestamp negativo");
        Position = position;
        Rotation = rotation;
        Timestamp = timestamp;
    }
}