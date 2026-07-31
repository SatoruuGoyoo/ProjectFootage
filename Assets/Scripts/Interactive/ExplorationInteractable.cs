using UnityEngine;
using UnityEngine.Events;

public class ExplorationInteractable : Interactable
{
    [Header("Subtitles")]
    [SerializeField] private SubtitleBlock subtitles;
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    [Header("Prompt")]
    [SerializeField] private string examinePrompt = "examinar";

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Events")]
    public UnityEvent OnExamineStarted;
    public UnityEvent OnExamineFinished;

    private int _index = -1;
    private bool _used;

    public override string PromptMessage => examinePrompt;
    public override bool CanInteract => !_used && lines != null && lines.Length > 0;
    public override bool BlockMovement => true;
    public override bool IsActive => _index >= 0;
    public override bool KeepProximityKeyWhenActive => true;

    public override void Interact()
    {
        _index++;

        if (_index >= lines.Length)
        {
            EndSequence();
            return;
        }

        if (_index == 0)
        {
            GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
            GameEvents.InteractPromptActivated(PromptMessage, ActiveIcon, KeepProximityKeyWhenActive);
            OnExamineStarted?.Invoke();
        }

        subtitles.Show(lines[_index], uiPosition);
    }

    private void EndSequence()
    {
        _index = -1;
        subtitles.Hide();
        if (oneTimeOnly) _used = true;

        GameEvents.InteractPromptDeactivated();
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);

        OnExamineFinished?.Invoke();
    }

    private void OnDisable()
    {
        if (_index >= 0) EndSequence();
    }
}