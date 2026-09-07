using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class ToggleInteractable : Interactable
{
    [Header("Prompts")]
    [SerializeField] private string promptOff = "encender";
    [SerializeField] private string promptOn = "apagar";

    [Header("Estado")]
    [SerializeField] private bool startsOn = false;
    [Tooltip("Apagar. Destildado para cosas que sólo se accionan, como llamar un ascensor.")]
    [SerializeField] private bool canTurnOff = true;
    [Tooltip("Después de usarse queda inutilizable.")]
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Audio 3D (opcional)")]
    [SerializeField] private World3DSource source;

    [Header("Subtítulos (opcional)")]
    [SerializeField] private SubtitleBlock subtitles;
    [SerializeField] private SubtitleEntry[] subtitleEntries;

    [Header("SFX de accionar (opcional)")]
    [SerializeField] private EventReference turnOnSound;
    [SerializeField] private EventReference turnOffSound;

    [Header("Events")]
    public UnityEvent OnTurnedOn;
    public UnityEvent OnTurnedOff;
    public UnityEvent OnFirstTurnedOn;

    private bool _isOn;
    private bool _used;
    private bool _turnedOnOnce;

    public override string PromptMessage => _isOn ? promptOn : promptOff;
    public override bool CanInteract => !_used && (!_isOn || canTurnOff);
    public override bool IsActive => _isOn;
    public override bool BlockMovement => false;

    private void Awake()
    {
        if (source == null) source = GetComponent<World3DSource>();
        if (startsOn) ApplyOn(silent: true);
    }

    public override void Interact()
    {
        if (!CanInteract) return;

        if (_isOn) TurnOff();
        else TurnOn();
    }

    public void TurnOn()
    {
        if (_isOn) return;
        ApplyOn(silent: false);

        if (!canTurnOff && oneTimeOnly) _used = true;
    }

    public void TurnOff()
    {
        if (!_isOn) return;

        _isOn = false;
        if (source != null && source.IsPlaying) source.Toggle();
        if (subtitles != null) subtitles.Hide();
        if (!turnOffSound.IsNull) RuntimeManager.PlayOneShot(turnOffSound, transform.position);

        if (oneTimeOnly) _used = true;

        OnTurnedOff?.Invoke();
    }

    private void ApplyOn(bool silent)
    {
        _isOn = true;

        if (source != null && !source.IsPlaying) source.Toggle();

        if (subtitles != null && subtitleEntries != null && subtitleEntries.Length > 0)
            subtitles.ShowSequence(subtitleEntries);

        if (!silent && !turnOnSound.IsNull)
            RuntimeManager.PlayOneShot(turnOnSound, transform.position);

        if (!_turnedOnOnce)
        {
            _turnedOnOnce = true;
            if (!silent) OnFirstTurnedOn?.Invoke();
        }

        if (!silent) OnTurnedOn?.Invoke();
    }
}