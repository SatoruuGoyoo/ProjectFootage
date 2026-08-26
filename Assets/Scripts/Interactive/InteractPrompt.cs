using System;
using UnityEngine;

public readonly struct InteractPrompt : IEquatable<InteractPrompt>
{
    public readonly string Message;
    public readonly Sprite Icon;
    public readonly bool ShowInteractKey;
    public readonly bool ShowCancelKey;
    public readonly bool Active;
    public readonly Transform Anchor;
    public readonly Vector3 Offset;
    public readonly bool InRange;
    public readonly bool ForceScreenPlacement;

    public InteractPrompt(string message, Sprite icon, bool showInteractKey, bool showCancelKey, bool active, Transform anchor, Vector3 offset, bool inRange, bool forceScreenPlacement)
    {
        Message = message;
        Icon = icon;
        ShowInteractKey = showInteractKey;
        ShowCancelKey = showCancelKey;
        Active = active;
        Anchor = anchor;
        Offset = offset;
        InRange = inRange;
        ForceScreenPlacement = forceScreenPlacement;
    }

    public bool IsVisible => !string.IsNullOrEmpty(Message);

    public InteractPromptKey Key => ShowInteractKey ? InteractPromptKey.Interact : InteractPromptKey.Cancel;

    public static InteractPrompt From(IInteractable target, bool inRange)
    {
        if (target == null) return default;

        bool active = target.IsActive;
        if (active && target.ActivePrompt == ActivePromptMode.Hidden) return default;

        return new InteractPrompt(
            target.PromptMessage,
            ResolveIcon(target, active, inRange),
            target.CanInteract,
            active,
            active,
            target.PromptAnchor,
            target.PromptOffset,
            inRange,
            active && target.ActivePrompt == ActivePromptMode.ScreenSlot);
    }

    private static Sprite ResolveIcon(IInteractable target, bool active, bool inRange)
    {
        if (active) return target.ActiveIcon;
        return inRange ? target.PromptIcon : target.DetectedIcon;
    }

    public bool Equals(InteractPrompt other) =>
        Message == other.Message
        && Icon == other.Icon
        && ShowInteractKey == other.ShowInteractKey
        && ShowCancelKey == other.ShowCancelKey
        && Active == other.Active
        && Anchor == other.Anchor
        && Offset == other.Offset
        && InRange == other.InRange
        && ForceScreenPlacement == other.ForceScreenPlacement;

    public override bool Equals(object obj) => obj is InteractPrompt other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(
        Message,
        Icon,
        Active,
        Anchor,
        Offset,
        InRange,
        ForceScreenPlacement,
        HashCode.Combine(ShowInteractKey, ShowCancelKey));
}