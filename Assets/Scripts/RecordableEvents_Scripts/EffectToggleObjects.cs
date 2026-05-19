using UnityEngine;

public class EffectToggleObjects : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private GameObject[] toEnable;
    [SerializeField] private GameObject[] toDisable;
    [SerializeField] private bool revertOnInterrupt = true;
    [SerializeField] private bool revertOnComplete = false;

    public void OnRecordingStarted() => Apply(true);
    public void OnRecordingProgress(float time) { }
    public void OnRecordingCompleted() { if (revertOnComplete) Apply(false); }
    public void OnRecordingInterrupted() { if (revertOnInterrupt) Apply(false); }

    private void Apply(bool active)
    {
        foreach (var go in toEnable) if (go != null) go.SetActive(active);
        foreach (var go in toDisable) if (go != null) go.SetActive(!active);
    }
}