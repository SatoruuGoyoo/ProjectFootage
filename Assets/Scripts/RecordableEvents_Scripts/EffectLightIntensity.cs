using UnityEngine;

public class EffectLightIntensity : MonoBehaviour, IRecordableEffect
{
    [SerializeField] private Light[] lights;
    [SerializeField, Range(0f, 1f)] private float targetMultiplier = 0.3f;
    [SerializeField] private float lerpSpeed = 2f;
    [SerializeField] private bool keepOnComplete = true;

    private float[] _originals;
    private float _target = 1f;
    private bool _active;

    private void Awake()
    {
        _originals = new float[lights.Length];
        for (int i = 0; i < lights.Length; i++)
            if (lights[i] != null) _originals[i] = lights[i].intensity;
    }

    public void OnRecordingStarted()
    {
        _target = targetMultiplier;
        _active = true;
    }

    public void OnRecordingProgress(float t) { }

    public void OnRecordingCompleted()
    {
        if (keepOnComplete) { _active = false; return; }
        _target = 1f;
    }

    public void OnRecordingInterrupted() => _target = 1f;
    private void Update()
    {
        if (!_active && Mathf.Approximately(_target, 1f)) return;

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] == null) continue;
            float goal = _originals[i] * _target;
            lights[i].intensity = Mathf.Lerp(lights[i].intensity, goal, Time.deltaTime * lerpSpeed);
        }
    }
}