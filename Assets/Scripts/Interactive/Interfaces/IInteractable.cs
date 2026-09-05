using UnityEngine;

public interface IInteractable
{
    string PromptMessage { get; }
    bool CanInteract { get; }
    bool IsActive { get; }
    bool BlockMovement { get; }
    Sprite DetectedIcon { get; }
    Sprite PromptIcon { get; }
    Sprite ActiveIcon { get; }
    Transform PromptAnchor { get; }
    Vector3 PromptOffset { get; }
    ActivePromptMode ActivePrompt { get; }
    void Interact();
    void Cancel();
}