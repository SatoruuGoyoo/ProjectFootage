using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class EffectFixedAudio : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private EventReference audioEvent;
    [SerializeField] private Transform sourceOverride;
    [SerializeField] private bool stopOnInterrupt = true;
    [SerializeField] private bool stopOnComplete = false;

    private EventInstance instance;
    private bool playing;

    private Transform Source => sourceOverride != null ? sourceOverride : transform;

    public void OnRecordingStarted()
    {
        if (audioEvent.IsNull) return;
        instance = FMODManager.Instance.CreateEventInstance(audioEvent);
        RuntimeManager.AttachInstanceToGameObject(instance, Source.gameObject);
        instance.start();
        playing = true;
    }

    public void OnRecordingProgress(float time) { }
    public void OnRecordingCompleted() { if (stopOnComplete) Stop(false); }
    public void OnRecordingInterrupted() { if (stopOnInterrupt) Stop(true); }

    private void Stop(bool immediate)
    {
        if (!playing) return;
        instance.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        instance.release();
        playing = false;
    }

    private void OnDestroy() => Stop(true);
}