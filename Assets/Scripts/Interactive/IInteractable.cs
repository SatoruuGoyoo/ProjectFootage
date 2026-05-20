using UnityEngine;

public interface IInteractable
{
    string PromptMessage { get; }
    bool CanInteract { get; }
    void Interact();
}
