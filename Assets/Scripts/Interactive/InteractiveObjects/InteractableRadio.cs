using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(World3DSource))]
public class InteractableRadio : Interactable
{
    [Header("Subtitles")]
    [SerializeField] private SubtitleBlock subtitles;
    [SerializeField] private SubtitleEntry[] subtitleEntries;

    [Header("Prompt")]
    [SerializeField] private string radioPrompt = "radio";

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Events")]
    public UnityEvent OnTurnedOn;
    public UnityEvent OnTurnedOff;

    private World3DSource _source;
    private bool _used;

    public override string PromptMessage => radioPrompt;
    public override bool CanInteract => !_used && !_source.IsPlaying;
    public override bool IsActive => _source.IsPlaying;
    public override bool BlockMovement => false;

    private void Awake()
    {
        _source = GetComponent<World3DSource>();
    }

    public override void Interact()
    {
        if (!CanInteract) return;

        _source.Toggle();

        if (subtitles != null && subtitleEntries != null && subtitleEntries.Length > 0)
            subtitles.ShowSequence(subtitleEntries);

        OnTurnedOn?.Invoke();
    }

    public override void Cancel()
    {
        if (!_source.IsPlaying) return;

        _source.Toggle();

        if (subtitles != null) subtitles.Hide();
        if (oneTimeOnly) _used = true;

        OnTurnedOff?.Invoke();
    }
}