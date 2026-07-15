using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("UI Position")]
    [SerializeField] protected UIPositioner.ScreenPosition uiPosition = UIPositioner.ScreenPosition.LowerCenter;

    [Header("Prompt Icon")]
    [SerializeField] protected Sprite promptIcon;

    public abstract string PromptMessage { get; }
    public abstract bool CanInteract { get; }
    public abstract bool BlockMovement { get; }
    public virtual Sprite PromptIcon => promptIcon;
    public abstract void Interact();
}