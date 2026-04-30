using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class IterationPlaybackAudio : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference audioEvent;

    [Header("Config")]
    [SerializeField] private int targetIteration;

    private EventInstance audioInstance;
    private bool isActive = false;

    private void OnEnable() => GameEvents.OnIterationChanged += OnIterationChanged;
    private void OnDisable() => GameEvents.OnIterationChanged -= OnIterationChanged;

    private void Start()
    {
        audioInstance = FMODManager.Instance.CreateEventInstance(audioEvent);
        isActive = PuzzleManager.Instance != null &&
                   PuzzleManager.Instance.CurrentIteration == targetIteration;
    }

    private void OnDestroy()
    {
        audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        audioInstance.release();
    }

    private void OnIterationChanged(int iteration)
    {
        isActive = iteration == targetIteration;
        if (!isActive) audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void OnPlaybackStarted(bool isResume = false)
    {
        if (!isActive) return;

        if (isResume)
        {
            audioInstance.setPaused(false);
        }
        else
        {
            audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            audioInstance.start();
        }
    }

    public void OnPlaybackPaused()
    {
        if (!isActive) return;
        audioInstance.setPaused(true);
    }

    public void OnPlaybackStopped()
    {
        if (!isActive) return;
        audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}