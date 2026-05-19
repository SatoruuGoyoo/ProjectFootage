using UnityEngine;

public class RecordableEvent : MonoBehaviour, ICamcorderTarget, ICenteredAware
{
    [Header("Identity")]
    [SerializeField] private string eventId;

    [Header("Behaviour")]
    [SerializeField] private float duration = 5f;
    [SerializeField] private bool repeatable = false;

    [Header("Target")]
    [SerializeField] private Transform targetOverride;

    private IRecordableEffect[] _effects;
    private RecordableEventState _state = RecordableEventState.Idle;
    private float _elapsed;
    private bool _camcorderRecording;
    private bool _isCentered;

    public string EventId => eventId;
    public bool Repeatable => repeatable;
    public RecordableEventState State => _state;
    public float NormalizedProgress => duration > 0f ? Mathf.Clamp01(_elapsed / duration) : 0f;

    public bool IsActive => _state != RecordableEventState.Completed || repeatable;
    public Transform TargetTransform => targetOverride != null ? targetOverride : transform;

    private void Awake()
    {
        _effects = GetComponentsInChildren<IRecordableEffect>(true);
    }

    private void OnEnable()
    {
        GameEvents.OnRecordingStarted += HandleRecordingStarted;
        GameEvents.OnRecordingStopped += HandleRecordingStopped;

        if (CamcorderLightSystem.Instance != null)
            CamcorderLightSystem.Instance.Register(this);

        if (RecordableEventManager.Instance != null)
            RecordableEventManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        GameEvents.OnRecordingStarted -= HandleRecordingStarted;
        GameEvents.OnRecordingStopped -= HandleRecordingStopped;

        if (CamcorderLightSystem.Instance != null)
            CamcorderLightSystem.Instance.Unregister(this);

        if (RecordableEventManager.Instance != null)
            RecordableEventManager.Instance.Unregister(this);
    }

    private void Update()
    {
        if(_state != RecordableEventState.Recording) return;

        _elapsed = Time.deltaTime;
        float time = Mathf.Clamp01(_elapsed / duration);

        foreach (var effect in _effects)
        {
            effect.OnRecordingProgress(time);
        }

        if(_elapsed >= duration)
        {
            Complete();
        }
    }

    public void OnCenteredChanged(bool centered)
    {
        _isCentered = centered;
        if (centered) TryStart();
        else Interrupt();
    }

    private void HandleRecordingStarted(RecordingSession session)
    {
        _camcorderRecording = true;
        TryStart();
    }

    private void HandleRecordingStopped()
    {
        _camcorderRecording = false;
        Interrupt();
    }

    private void TryStart()
    {
        if (_state == RecordableEventState.Completed && !repeatable) return;
        if (_state == RecordableEventState.Recording) return;
        if (!_isCentered || !_camcorderRecording) return;

        _state = RecordableEventState.Recording;
        _elapsed = 0f;

        foreach (var effect in _effects)
        {
            effect.OnRecordingStarted();
        }

        GameEvents.RecordableEventStarted(eventId);
    }

    private void Interrupt()
    {
        if(_state != RecordableEventState.Recording) return;

        _state = RecordableEventState.Idle;
        _elapsed = 0f;

        foreach (var effect in _effects)
        {
            effect.OnRecordingInterrupted();
        }

        GameEvents.RecordableEventInterrupted(eventId);

        
    }

    private void Complete()
    {
        _state = RecordableEventState.Completed;

        foreach (var effect in _effects) effect.OnRecordingCompleted();
        GameEvents.RecordableEventCompleted(eventId);

        if (repeatable) _state = RecordableEventState.Idle;
    }
}
