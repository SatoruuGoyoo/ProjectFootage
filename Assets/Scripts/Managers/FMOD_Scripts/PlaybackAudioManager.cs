using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class PlaybackAudioManager : MonoBehaviour
{
    public static PlaybackAudioManager Instance { get; private set; }

    [Header("FMOD")]
    [SerializeField] private EventReference reverseAudioEvent;

    [Header("Config")]
    [SerializeField] private int activeIteration = 2;

    private EventInstance reverseInstance;
    private bool isActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable() => GameEvents.OnIterationChanged += OnIterationChanged;
    private void OnDisable() => GameEvents.OnIterationChanged -= OnIterationChanged;

    private void Start()
    {
        reverseInstance = FMODManager.Instance.CreateEventInstance(reverseAudioEvent);
    }

    private void OnDestroy()
    {
        reverseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        reverseInstance.release();
    }

    private void OnIterationChanged(int iteration)
    {
        bool wasActive = isActive;
        isActive = iteration >= activeIteration;
        if (wasActive && !isActive)
            reverseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void OnPlaybackStarted(bool isResume = false)
    {
        if (!isActive) return;

        if (isResume)
        {
            reverseInstance.setPaused(false);
        }
        else
        {
            reverseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            reverseInstance.start();
        }
        reverseInstance.setParameterByName("Speed", 0.50f);
    }

    public void OnPlaybackStopped()
    {
        if (!isActive) return;
        reverseInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void OnPlaybackPaused()
    {
        if (!isActive) return;
        reverseInstance.setPaused(true); // solo pausa
    }

    public void OnRFF(bool isRewind)
    {
        if (!isActive) return;

        PLAYBACK_STATE state;
        reverseInstance.getPlaybackState(out state);

        if (state == PLAYBACK_STATE.STOPPED || state == PLAYBACK_STATE.STOPPING)
            reverseInstance.start();
        else
            reverseInstance.setPaused(false);

        reverseInstance.setParameterByName("Speed", isRewind ? -1f : 2f);
    }

    public void OnRFFStopped()
    {
        reverseInstance.setParameterByName("Speed", 0.50f);
    }
}