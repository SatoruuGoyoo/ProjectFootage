using UnityEngine;
using FMODUnity;
using FMOD.Studio;
public class HallwayAudioManager : MonoBehaviour
{
    public static HallwayAudioManager Instance { get; private set; }

    [Header("FMOD")]
    [SerializeField] private EventReference ambienceEvent;

    private EventInstance ambienceInstance;
    private int _currentIteration = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ambienceInstance = FMODManager.Instance.CreateEventInstance(ambienceEvent);
        ambienceInstance.start();
        RuntimeManager.StudioSystem.setParameterByName("Iteration", _currentIteration); // Global parameter to control iteration-based changes in the ambience track
    }

    private void OnDestroy()
    {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        ambienceInstance.release();
    }

    public void AdvanceIteration()
    {
        _currentIteration = Mathf.Clamp( _currentIteration + 1, 0, 2 );
        RuntimeManager.StudioSystem.setParameterByName("Iteration", (float)_currentIteration); // Update the global parameter to reflect the new iteration
    }
}