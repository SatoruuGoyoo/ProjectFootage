using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlaybackAudioManager : MonoBehaviour
{
    public static PlaybackAudioManager Instance { get; private set; }

    [Header("FMOD")]
    [SerializeField] private EventReference reverseAudioEvent;

    [Header("Config")]
    [SerializeField] private int activeIteration = 2; // index of iteration to play reverse audio for (0-based)

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
        isActive = iteration >= activeIteration;
    }

    // Called by CamcorderMenuController on Rewind/FastForward
    public void OnRFF()
    {
        if (!isActive) return;

        PLAYBACK_STATE state;
        reverseInstance.getPlaybackState(out state);

        if (state != PLAYBACK_STATE.PLAYING)
            reverseInstance.start();
    }

    // Called when R/FF stops
    public void OnRFFStopped()
    {
        reverseInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
