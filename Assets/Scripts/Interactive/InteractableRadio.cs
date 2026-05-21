using UnityEngine;

[RequireComponent(typeof(World3DSource))]
public class InteractableRadio : MonoBehaviour, IInteractable
{
    [SerializeField] private string onPrompt = "";
    [SerializeField] private string offPrompt = "";

    private World3DSource _source;

    public string PromptMessage => _source.IsPlaying ? offPrompt : onPrompt;
    public bool CanInteract => true;

    private void Awake()
    {
        _source = GetComponent<World3DSource>();
    }

    public void Interact()
    {
        _source.Toggle();
    }



}
