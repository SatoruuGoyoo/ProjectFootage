using UnityEngine;

public interface ISpatialAudioSource
{
    Vector3 WorldPosition { get; }

    string FMODPath { get; }

    bool Is3D { get; }

    float MaxAudibleDistance { get; }

    bool IsActiveInScene { get; }

    bool TryGetTimelinePosition(out int milliseconds);

    float EvaluateDistanceFalloff(float normalizedDistance);
}