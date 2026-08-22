using System;
using UnityEngine;

public readonly struct InteractPrompt : IEquatable<InteractPrompt>
{
    public readonly string Message;
    public readonly Sprite Icon;
    public readonly InteractPromptKey Key;
    public readonly bool Active;
    public readonly Transform Anchor;
    public readonly Vector3 Offset;
    public readonly bool InRange;

    public InteractPrompt(string message, Sprite icon, InteractPromptKey key, bool active, Transform anchor, Vector3 offset, bool inRange)
    {
        Message = message;
        Icon = icon;
        Key = key;
        Active = active;
        Anchor = anchor;
        Offset = offset;
        InRange = inRange;
    }

    public bool IsVisible => !string.IsNullOrEmpty(Message);

    public static InteractPrompt From(IInteractable target, bool inRange)
    {
        if (target == null) return default;

        bool active = target.IsActive;

        return new InteractPrompt(
            target.PromptMessage,
            active ? target.ActiveIcon : target.PromptIcon,
            target.CanInteract ? InteractPromptKey.Interact : InteractPromptKey.Cancel,
            active,
            target.PromptAnchor,
            target.PromptOffset,
            inRange);
    }

    public bool Equals(InteractPrompt other) =>
        Message == other.Message
        && Icon == other.Icon
        && Key == other.Key
        && Active == other.Active
        && Anchor == other.Anchor
        && Offset == other.Offset
        && InRange == other.InRange;

    public override bool Equals(object obj) => obj is InteractPrompt other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Message, Icon, Key, Active, Anchor, Offset, InRange);
}