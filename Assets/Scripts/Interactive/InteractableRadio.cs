using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(World3DSource))]
public class InteractableRadio : MonoBehaviour, IInteractable
{
    [Header("Subtitles")]
    [SerializeField] private SubtitleBlock subtitles;
    [SerializeField] private SubtitleEntry[] subtitleEntries;

    [Header("Events")]
    public UnityEvent OnTurnedOn;
    public UnityEvent OnTurnedOff;

    private World3DSource _source;

    public string PromptMessage => "radio";
    public bool CanInteract => true;
    public bool BlockMovement => false;

    private void Awake()
    {
        _source = GetComponent<World3DSource>();
    }

    public void Interact()
    {
        _source.Toggle();

        if (subtitles != null)
        {
            if (_source.IsPlaying)
                subtitles.ShowSequence(subtitleEntries);
            else
                subtitles.Hide();
        }

        if (_source.IsPlaying) OnTurnedOn?.Invoke();
        else OnTurnedOff?.Invoke();
    }
}