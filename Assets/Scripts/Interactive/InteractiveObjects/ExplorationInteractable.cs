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
    [Tooltip("Ícono al llegar a la última línea. Vacío usa el Active Icon normal.")]
    [SerializeField] private Sprite lastLineIcon;

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = false;

    [Header("Events")]
    public UnityEvent OnExamineStarted;
    public UnityEvent OnExamineFinished;

    private int _index = -1;
    private bool _used;

    private bool HasLines => lines != null && lines.Length > 0;
    private bool OnLastLine => _index >= 0 && _index >= lines.Length - 1;

    public override string PromptMessage => examinePrompt;
    public override bool CanInteract => !_used && HasLines && !OnLastLine;
    public override bool IsActive => _index >= 0;
    public override bool BlockMovement => true;
    public override Sprite ActiveIcon => OnLastLine && lastLineIcon != null ? lastLineIcon : base.ActiveIcon;

    public override void Interact()
    {
        if (!CanInteract) return;

        if (_index < 0)
        {
            _index = 0;
            EnterInteractionMode();
            OnExamineStarted?.Invoke();
        }
        else
        {
            _index++;
        }

        subtitles.Show(lines[_index], uiPosition);
    }

    public override void Cancel()
    {
        if (_index < 0) return;
        EndSequence();
    }

    private void EndSequence()
    {
        _index = -1;
        subtitles.Hide();
        if (oneTimeOnly) _used = true;

        ExitInteractionMode();
        OnExamineFinished?.Invoke();
    }

    private void OnDisable()
    {
        if (_index >= 0) EndSequence();
    }
}