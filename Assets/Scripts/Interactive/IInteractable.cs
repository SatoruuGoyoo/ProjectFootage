using UnityEngine;

public interface IInteractable
{
    string PromptMessage { get; }
    bool CanInteract { get; }
    bool IsActive { get; }
    bool BlockMovement { get; }
    Sprite PromptIcon { get; }
    Sprite ActiveIcon { get; }
    void Interact();
    void Cancel();
}