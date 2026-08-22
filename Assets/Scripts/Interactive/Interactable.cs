using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("UI Position")]
    [SerializeField] protected UIPositioner.ScreenPosition uiPosition = UIPositioner.ScreenPosition.LowerCenter;

    [Header("Prompt Icons")]
    [SerializeField] protected Sprite promptIcon;
    [SerializeField] protected Sprite activeIcon;

    [Header("World Prompt")]
    [SerializeField] protected Transform promptAnchor;
    [SerializeField] protected Vector3 promptOffset = new Vector3(0f, 0.25f, 0f);

    public abstract string PromptMessage { get; }
    public abstract bool CanInteract { get; }
    public abstract bool BlockMovement { get; }

    public virtual bool IsActive => false;
    public virtual Sprite PromptIcon => promptIcon;
    public virtual Sprite ActiveIcon => activeIcon;
    public virtual Transform PromptAnchor => promptAnchor != null ? promptAnchor : transform;
    public virtual Vector3 PromptOffset => promptOffset;

    public abstract void Interact();

    public virtual void Cancel() { }

    protected static void EnterInteractionMode() => GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
    protected static void ExitInteractionMode() => GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);

    protected virtual void OnDrawGizmosSelected()
    {
        Transform anchor = PromptAnchor;
        if (anchor == null) return;

        Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
        Vector3 point = anchor.position + PromptOffset;
        Gizmos.DrawWireSphere(point, 0.06f);
        Gizmos.DrawLine(anchor.position, point);
    }
}