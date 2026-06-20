using UnityEngine;

public interface IInteractable
{
    string PromptMessage { get; }
    bool CanInteract { get; }
    bool BlockMovement { get; }
    void Interact();
}
