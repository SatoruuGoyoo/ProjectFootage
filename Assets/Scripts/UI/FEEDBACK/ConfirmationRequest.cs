using System;
using System.Collections.Generic;

public readonly struct ConfirmationRequest
{
    public readonly string Message;
    public readonly Action OnConfirm;
    public readonly Action OnDecline;
    public readonly UIPositioner.ScreenPosition Position;
    public readonly IReadOnlyList<InteractionInstruction> Instructions;
    public readonly UIPositioner.ScreenPosition InstructionsPosition;
    public readonly bool OverridesInstructions;

    public ConfirmationRequest(
        string message,
        Action onConfirm,
        Action onDecline,
        UIPositioner.ScreenPosition position,
        IReadOnlyList<InteractionInstruction> instructions,
        UIPositioner.ScreenPosition instructionsPosition,
        bool overridesInstructions)
    {
        Message = message;
        OnConfirm = onConfirm;
        OnDecline = onDecline;
        Position = position;
        Instructions = instructions;
        InstructionsPosition = instructionsPosition;
        OverridesInstructions = overridesInstructions;
    }
}