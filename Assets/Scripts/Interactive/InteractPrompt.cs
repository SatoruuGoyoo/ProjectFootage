using System;
using UnityEngine;

public readonly struct InteractPrompt : IEquatable<InteractPrompt>
{
    public readonly string Message;
    public readonly Sprite Icon;
    public readonly InteractPromptKey Key;
    public readonly bool Active;

    public InteractPrompt(string message, Sprite icon, InteractPromptKey key, bool active)
    {
        Message = message;
        Icon = icon;
        Key = key;
        Active = active;
    }

    public bool IsVisible => !string.IsNullOrEmpty(Message);

    public static InteractPrompt From(IInteractable target)
    {
        if (target == null) return default;

        bool active = target.IsActive;

        return new InteractPrompt(
            target.PromptMessage,
            active ? target.ActiveIcon : target.PromptIcon,
            target.CanInteract ? InteractPromptKey.Interact : InteractPromptKey.Cancel,
            active);
    }

    public bool Equals(InteractPrompt other) =>
        Message == other.Message && Icon == other.Icon && Key == other.Key && Active == other.Active;

    public override bool Equals(object obj) => obj is InteractPrompt other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Message, Icon, Key, Active);
}