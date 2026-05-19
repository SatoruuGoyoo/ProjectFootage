using UnityEngine;
using UnityEngine.Events;

public class EffectUnityEvent : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private UnityEvent onStarted;
    [SerializeField] private UnityEvent onCompleted;
    [SerializeField] private UnityEvent onInterrupted;

    public void OnRecordingStarted() => onStarted?.Invoke();
    public void OnRecordingProgress(float t) { }
    public void OnRecordingCompleted() => onCompleted?.Invoke();
    public void OnRecordingInterrupted() => onInterrupted?.Invoke();
}