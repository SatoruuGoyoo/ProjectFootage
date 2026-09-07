using System;
using UnityEngine;

public readonly struct InteractPrompt : IEquatable<InteractPrompt>
{
    public readonly string Message;
    public readonly Sprite DetectedIcon;
    public readonly Sprite PromptIcon;
    public readonly Sprite ActiveIcon;
    public readonly bool ShowKey;
    public readonly bool Active;
    public readonly bool InRange;
    public readonly bool ForceScreenPlacement;
    public readonly Transform Anchor;
    public readonly Vector3 Offset;

    public InteractPrompt(string message, Sprite detectedIcon, Sprite promptIcon, Sprite activeIcon,
        bool showKey, bool active, bool inRange, bool forceScreenPlacement, Transform anchor, Vector3 offset)
    {
        Message = message;
        DetectedIcon = detectedIcon;
        PromptIcon = promptIcon;
        ActiveIcon = activeIcon;
        ShowKey = showKey;
        Active = active;
        InRange = inRange;
        ForceScreenPlacement = forceScreenPlacement;
        Anchor = anchor;
        Offset = offset;
    }

    public bool IsVisible => !string.IsNullOrEmpty(Message);

    public static InteractPrompt From(IInteractable target, bool inRange)
    {
        if (target == null) return default;

        bool active = target.IsActive;
        if (active && target.ActivePrompt == ActivePromptMode.Hidden) return default;

        return new InteractPrompt(
            target.PromptMessage,
            target.DetectedIcon,
            target.PromptIcon,
            target.ActiveIcon,
            target.CanInteract,
            active,
            inRange,
            active && target.ActivePrompt == ActivePromptMode.ScreenSlot,
            target.PromptAnchor,
            target.PromptOffset);
    }

    public bool Equals(InteractPrompt other) =>
        Message == other.Message
        && DetectedIcon == other.DetectedIcon
        && PromptIcon == other.PromptIcon
        && ActiveIcon == other.ActiveIcon
        && ShowKey == other.ShowKey
        && Active == other.Active
        && InRange == other.InRange
        && ForceScreenPlacement == other.ForceScreenPlacement
        && Anchor == other.Anchor
        && Offset == other.Offset;

    public override bool Equals(object obj) => obj is InteractPrompt other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        Message,
        DetectedIcon,
        PromptIcon,
        ActiveIcon,
        Anchor,
        Offset,
        HashCode.Combine(ShowKey, Active, InRange, ForceScreenPlacement));
}