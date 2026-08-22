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

    public virtual bool IsActive => false;
    public virtual Sprite PromptIcon => promptIcon;
    public virtual Sprite ActiveIcon => activeIcon;

    public abstract void Interact();

    public virtual void Cancel() { }

    protected static void EnterInteractionMode() => GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
    protected static void ExitInteractionMode() => GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
}