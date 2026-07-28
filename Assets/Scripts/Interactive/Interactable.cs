using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("UI Position")]
    [SerializeField] protected UIPositioner.ScreenPosition uiPosition = UIPositioner.ScreenPosition.LowerCenter;

    [Header("Prompt Icons")]
    [SerializeField] protected Sprite promptIcon;
    [SerializeField] protected Sprite activeIcon;

    public abstract string PromptMessage { get; }
    public abstract bool CanInteract { get; }
    public abstract bool BlockMovement { get; }
    public virtual Sprite PromptIcon => promptIcon;
    public virtual bool IsActive => false;
    public virtual Sprite ActiveIcon => activeIcon;
    public virtual bool KeepProximityKeyWhenActive => false;
    public abstract void Interact();
}