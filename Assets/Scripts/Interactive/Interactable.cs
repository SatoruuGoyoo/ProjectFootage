using UnityEngine;

public abstract class Interactable : MonoBehaviour, IInteractable
{
    [Header("UI Position")]
    [SerializeField] protected UIPositioner.ScreenPosition uiPosition = UIPositioner.ScreenPosition.LowerCenter;

    [Header("Prompt Icons")]
    [Tooltip("Detectado pero todavía fuera de rango.")]
    [SerializeField] protected Sprite detectedIcon;
    [Tooltip("Dentro de rango, listo para interactuar.")]
    [SerializeField] protected Sprite promptIcon;
    [Tooltip("Mientras la interacción está en curso.")]
    [SerializeField] protected Sprite activeIcon;

    [Header("World Prompt")]
    [SerializeField] protected Transform promptAnchor;
    [SerializeField] protected Vector3 promptOffset = new Vector3(0f, 0.25f, 0f);
    [Tooltip("Qué hace el prompt mientras este interactuable está activo. Hidden para los que abren su propio panel con la tecla dibujada adentro.")]
    [SerializeField] protected ActivePromptMode activePrompt = ActivePromptMode.KeepWorld;

    public abstract string PromptMessage { get; }
    public abstract bool CanInteract { get; }
    public abstract bool BlockMovement { get; }

    public virtual bool IsActive => false;
    public virtual Sprite DetectedIcon => detectedIcon != null ? detectedIcon : promptIcon;
    public virtual Sprite PromptIcon => promptIcon;
    public virtual Sprite ActiveIcon => activeIcon;
    public virtual Transform PromptAnchor => promptAnchor != null ? promptAnchor : transform;
    public virtual Vector3 PromptOffset => promptOffset;
    public virtual ActivePromptMode ActivePrompt => activePrompt;

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