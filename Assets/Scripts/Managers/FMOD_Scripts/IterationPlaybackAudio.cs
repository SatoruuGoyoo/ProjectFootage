using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class IterationPlaybackAudio : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference audioEvent;

    [Header("Config")]
    [SerializeField] private int targetIteration = 1; // index of iteration to play audio for (0-based)

    private EventInstance audioInstance;
    private bool isActive = false;

    private void OnEnable()
    {
        GameEvents.OnIterationChanged += OnIterationChanged;
        GameEvents.OnPlaybackEnded += OnPlaybackStopped;
    }

    private void OnDisable()
    {
        GameEvents.OnIterationChanged -= OnIterationChanged;
        GameEvents.OnPlaybackEnded -= OnPlaybackStopped;
    }

    private void Start()
    {
        audioInstance = FMODManager.Instance.CreateEventInstance(audioEvent);
    }

    private void OnDestroy()
    {
        audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        audioInstance.release();
    }

    private void OnIterationChanged(int iteration)
    {
        isActive = iteration == targetIteration;
        if(!isActive) audioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void OnPlaybackStarted()
    {
        if (!isActive) return;
        PLAYBACK_STATE state;
        audioInstance.getPlaybackState(out state);
        if (state != PLAYBACK_STATE.PLAYING)
            audioInstance.start();
    }

    public void OnPlaybackStopped()
    {
        audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
