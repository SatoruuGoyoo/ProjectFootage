using UnityEngine;
using UnityEngine.Video;

public interface ICamcorderTarget
{
    bool IsActive { get; }
    Transform TargetTransform { get; }
    float DetectionRadius { get; }

    bool TryGetLiveActionClip(out VideoClip clip);
}