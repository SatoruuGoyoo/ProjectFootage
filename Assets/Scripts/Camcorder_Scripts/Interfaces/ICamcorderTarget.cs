using UnityEngine;

public interface ICamcorderTarget
{
  bool IsActive { get; }
  Transform TargetTransform { get; }

  float DetectionRadius { get; }

}
