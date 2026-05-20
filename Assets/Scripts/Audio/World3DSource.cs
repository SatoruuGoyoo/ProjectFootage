using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class World3DSource : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private EventReference eventReference;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;

    [Header("Debug")]
    [SerializeField] private float gizmoRange = 10f;

    private EventInstance _instance;
    private bool _isPlaying;

    public bool IsPlaying => _isPlaying;

    private void Start()
    {
        if (playOnStart) Play();
    }

    public void Play()
    {
        if (_isPlaying) return;
        if (eventReference.IsNull) return;

        _instance = FMODManager.Instance.CreateEventInstance(eventReference);
        RuntimeManager.AttachInstanceToGameObject(_instance, transform);
        _instance.start();
        _isPlaying = true;
    }

    public void Stop(bool immediate = false)
    {
        if (!_isPlaying) return;
        _instance.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _instance.release();
        _isPlaying = false;
    }

    public void Toggle()
    {
        if (_isPlaying) Stop();
        else Play();
    }

    private void Update()
    {
        if (!_isPlaying || loop) return;
        _instance.getPlaybackState(out PLAYBACK_STATE state);
        if (state == PLAYBACK_STATE.STOPPED)
        {
            _instance.release();
            _isPlaying = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, gizmoRange);
    }

    private void OnDestroy() => Stop(true);
}
