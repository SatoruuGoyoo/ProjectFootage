using UnityEngine;

public class EffectToggleObjects : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private GameObject[] toEnable;
    [SerializeField] private GameObject[] toDisable;
    [SerializeField, Range(0f, 1f)] private float triggerAt = 0f;
    [SerializeField] private bool revertOnInterrupt = true;
    [SerializeField] private bool revertOnComplete = false;

    private bool _applied;
    public void OnRecordingStarted() => _applied = false;
    public void OnRecordingProgress(float time)
    {
        if (_applied) return;
        if (time >= triggerAt)
        {
            Apply(true);
            _applied = true;
        }
    }
    public void OnRecordingCompleted() { if (revertOnComplete) Apply(false); }
    public void OnRecordingInterrupted() { if (revertOnInterrupt) Apply(false); }

    private void Apply(bool active)
    {
        foreach (var go in toEnable) if (go != null) go.SetActive(active);
        foreach (var go in toDisable) if (go != null) go.SetActive(!active);
    }
}