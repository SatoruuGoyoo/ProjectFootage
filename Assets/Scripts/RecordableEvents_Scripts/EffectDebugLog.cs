using UnityEngine;

public class EffectDebugLog : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private string label = "Event";

    public void OnRecordingStarted() => Debug.Log($"[{label}] STARTED");
    public void OnRecordingProgress(float t) { }
    public void OnRecordingCompleted() => Debug.Log($"[{label}] COMPLETED");
    public void OnRecordingInterrupted() => Debug.Log($"[{label}] INTERRUPTED");
}