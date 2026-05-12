using System;
using System.Collections.Generic;
using UnityEngine;

public class RecordingSession
{
    public float Duration { get; private set; }
    public bool IsCompleted { get; private set; }

    // ── Video ─────────────────────────────────────────────────
    private readonly List<VideoFrame> _videoFrames = new List<VideoFrame>();
    public IReadOnlyList<VideoFrame> VideoFrames => _videoFrames;

    // ── Camera Transform ──────────────────────────────────────
    private readonly List<CameraTransformFrame> _cameraFrames = new List<CameraTransformFrame>();
    public IReadOnlyList<CameraTransformFrame> CameraFrames => _cameraFrames;

    // ── Audio continuo (cuchara, radio, etc) ──────────────────
    private readonly List<RecordedAudioTrack> _audioTracks = new List<RecordedAudioTrack>();
    public IReadOnlyList<RecordedAudioTrack> AudioTracks => _audioTracks;

    // ── One-shots (SFX puntuales, jumpscares, etc) ────────────
    private readonly List<RecordedOneShotEvent> _oneShotEvents = new List<RecordedOneShotEvent>();
    public IReadOnlyList<RecordedOneShotEvent> OneShotEvents => _oneShotEvents;

    // ── Escritura ──────────────────────────────────────────────

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

    public void RegisterAudioTrack(RecordedAudioTrack track)
    {
        if (IsCompleted) return;
        _audioTracks.Add(track);
    }

    public void RegisterOneShot(RecordedOneShotEvent evt)
    {
        if (IsCompleted) return;
        _oneShotEvents.Add(evt);
    }

    public void Complete(float duration)
    {
        if (duration < 0) throw new ArgumentException("Duración negativa");
        Duration = duration;
        IsCompleted = true;
    }

    // Video & Camera 

    public VideoFrame? GetFrameAtTime(float time)
    {
        if (_videoFrames.Count == 0) return null;
        return BinarySearch(_videoFrames, f => f.Timestamp, time);
    }

    public CameraTransformFrame? GetCameraAtTime(float time)
    {
        if (_cameraFrames.Count == 0) return null;
        return BinarySearch(_cameraFrames, f => f.Timestamp, time);
    }

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

// Structs 

public readonly struct VideoFrame
{
    public readonly byte[] PixelData;
    public readonly float Timestamp;

    public VideoFrame(byte[] pixelData, float timestamp)
    {
        if (timestamp < 0) throw new ArgumentException("Timestamp negativo");
        PixelData = pixelData;
        Timestamp = timestamp;
    }
}

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

public struct RecordedAudioTrack
{
    public string FMODPath;
    public float StartTime;
    public int FMODTimelinePosition;
    public bool Is3D;
    public Vector3 Position;
    public List<AudioVolumeKeyFrame> VolumeKeyframes;
}

public struct RecordedOneShotEvent
{
    public string FMODPath;
    public float Timestamp;   
    public Vector3 Position; 
    public float Volume;      
}