using System;
using System.Collections.Generic;
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
    [Tooltip("Qué hace el prompt mientras este interactuable está activo.")]
    [SerializeField] protected ActivePromptMode activePrompt = ActivePromptMode.KeepWorld;

    [Header("Instrucciones")]
    [Tooltip("Se muestran mientras la interacción está activa. Vacío no muestra la barra.")]
    [SerializeField] protected InteractionInstruction[] instructions;
    [Tooltip("Dónde se posiciona la barra de instrucciones de este interactuable.")]
    [SerializeField] protected UIPositioner.ScreenPosition instructionsPosition = UIPositioner.ScreenPosition.LowerRight;

    [Header("Instrucciones de la confirmación")]
    [Tooltip("Pisa las instrucciones que trae el ConfirmationUI, sólo para las confirmaciones que pida este interactuable.")]
    [SerializeField] protected bool overrideConfirmationInstructions = false;
    [SerializeField] protected InteractionInstruction[] confirmationInstructions;
    [SerializeField] protected UIPositioner.ScreenPosition confirmationInstructionsPosition = UIPositioner.ScreenPosition.LowerCenter;

    public abstract string PromptMessage { get; }
    public abstract bool CanInteract { get; }
    public abstract bool BlockMovement { get; }

    public virtual bool IsActive => false;
    public virtual Sprite DetectedIcon => detectedIcon;
    public virtual Sprite PromptIcon => promptIcon;
    public virtual Sprite ActiveIcon => activeIcon;
    public virtual ActivePromptMode ActivePrompt => activePrompt;
    public virtual IReadOnlyList<InteractionInstruction> Instructions => instructions;
    public virtual UIPositioner.ScreenPosition InstructionsPosition => instructionsPosition;
    public virtual Transform PromptAnchor => promptAnchor != null ? promptAnchor : transform;
    public virtual Vector3 PromptOffset => promptOffset;

    public abstract void Interact();

    protected void RequestConfirmation(string message, Action onConfirm, Action onDecline = null)
    {
        GameEvents.RequestConfirmation(new ConfirmationRequest(
            message,
            onConfirm,
            onDecline,
            uiPosition,
            confirmationInstructions,
            confirmationInstructionsPosition,
            overrideConfirmationInstructions));
    }

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