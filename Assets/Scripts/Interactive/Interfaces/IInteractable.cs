using System.Collections.Generic;
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
    ActivePromptMode ActivePrompt { get; }
    IReadOnlyList<InteractionInstruction> Instructions { get; }
    UIPositioner.ScreenPosition InstructionsPosition { get; }
    Transform PromptAnchor { get; }
    Vector3 PromptOffset { get; }
    void Interact();
}