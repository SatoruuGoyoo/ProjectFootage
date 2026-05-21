using UnityEngine;

public interface IRecordableEffect
{
  void OnRecordingStarted();
  void OnRecordingProgress(float normalizedTime);
  void OnRecordingCompleted();
  void OnRecordingInterrupted();
}
