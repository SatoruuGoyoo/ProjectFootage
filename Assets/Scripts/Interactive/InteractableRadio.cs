using UnityEngine;

[RequireComponent(typeof(World3DSource))]
public class InteractableRadio : MonoBehaviour, IInteractable
{
    [Header("Subtitles")]
    [SerializeField] private SubtitleBlock subtitles;
    [TextArea(3, 10)]
    [SerializeField] private string subtitleText = "";

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

        if (subtitles == null) return;

        if (_source.IsPlaying)
            subtitles.Show(subtitleText);
        else
            subtitles.Hide();
    }
}