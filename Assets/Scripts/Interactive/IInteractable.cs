using UnityEngine;

public interface IInteractable
{
    string PromptMessage { get; }
    bool CanInteract { get; }
    bool BlockMovement { get; }
    Sprite PromptIcon { get; }
    bool IsActive { get; }
    Sprite ActiveIcon { get; }
    bool KeepProximityKeyWhenActive { get; }
    void Interact();
}