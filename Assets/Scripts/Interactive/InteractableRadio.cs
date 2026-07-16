using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(World3DSource))]
public class InteractableRadio : Interactable
{
    [Header("Subtitles")]
    [SerializeField] private SubtitleBlock subtitles;
    [SerializeField] private SubtitleEntry[] subtitleEntries;

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Events")]
    public UnityEvent OnTurnedOn;
    public UnityEvent OnTurnedOff;

    private World3DSource _source;
    private bool _used;

    public override string PromptMessage => "radio";
    public override bool CanInteract => !_used;
    public override bool BlockMovement => false;
    public override bool IsActive => _source.IsPlaying;

    private void Awake()
    {
        _source = GetComponent<World3DSource>();
    }

    public override void Interact()
    {
        if (_used) return;
        if (oneTimeOnly) _used = true;

        _source.Toggle();

        if (subtitles != null)
        {
            if (_source.IsPlaying)
                subtitles.ShowSequence(subtitleEntries);
            else
                subtitles.Hide();
        }

        if (_source.IsPlaying)
            OnTurnedOn?.Invoke();
        else
            OnTurnedOff?.Invoke();
    }
}