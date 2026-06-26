using System;

public static class GameEvents
{
    // Player
    public static event Action OnPlayerDeath;
    public static event Action<float> OnHealthChanged;

    // Interaction
    public static event Action<string> OnInteractPromptChanged;

    // Items
    public static event Action<string> OnItemCollected;
    public static event Action<string> OnFeedbackMessage;

    // Player Mode
    public static event Action<PlayerMode> OnPlayerModeChanged;

    // Control Scheme
    public static event Action<ControlScheme> OnControllerSchemeChanged;

    // Pause
    public static event Action<bool> OnPauseChanged;

    // Camcorder
    public static event Action OnCamcorderPickedUp;
    public static event Action<RecordingSession> OnRecordingStarted;
    public static event Action OnRecordingStopped;
    public static event Action<int> OnFrameChanged;
    public static event Action<bool> OnCamcorderLightChanged;
    public static event Action OnPlaybackEnded;
    public static event Action<CamcorderZone> OnZoneChanged;

    // Recordable Events
    public static event Action<string> OnRecordableEventStarted;
    public static event Action<string> OnRecordableEventCompleted;
    public static event Action<string> OnRecordableEventInterrupted;
    public static event Action<string> OnRecordableEventWatched;


    // Interact Prompt State
    public static event Action<string> OnInteractPromptActivated;
    public static event Action OnInteractPromptDeactivated;

    public static void InteractPromptActivated(string promptType) => OnInteractPromptActivated?.Invoke(promptType);
    public static void InteractPromptDeactivated() => OnInteractPromptDeactivated?.Invoke();
    // Confirmation
    // onConfirm  → called if the player presses YES (E)
    // onDecline  → called if the player presses NO  (F) or walks away
    public static event Action<string, Action, Action> OnConfirmationRequested;
    public static event Action OnConfirmationClosed;

    // Readable
    public static event Action<UnityEngine.Sprite, string> OnReadableOpened;
    public static event Action OnReadableClosed;

    // ── PLAYER ───────────────────────────────────────────────────────────────
    public static void PlayerDied() => OnPlayerDeath?.Invoke();
    public static void HealthChanged(float newHealth) => OnHealthChanged?.Invoke(newHealth);
    public static void PlayerModeChanged(PlayerMode newMode) => OnPlayerModeChanged?.Invoke(newMode);
    public static void ControllerSchemeChanged(ControlScheme scheme) => OnControllerSchemeChanged?.Invoke(scheme);

    // ── INTERACTION ──────────────────────────────────────────────────────────
    public static void InteractPromptChanged(string newPrompt) => OnInteractPromptChanged?.Invoke(newPrompt);

    // ── ITEMS ────────────────────────────────────────────────────────────────
    public static void ItemCollected(string itemId) => OnItemCollected?.Invoke(itemId);
    public static void FeedbackMessage(string message) => OnFeedbackMessage?.Invoke(message);

    // ── CAMCORDER ────────────────────────────────────────────────────────────
    public static void CamcorderPickedUp() => OnCamcorderPickedUp?.Invoke();
    public static void RecordingStarted(RecordingSession session) => OnRecordingStarted?.Invoke(session);
    public static void RecordingStopped() => OnRecordingStopped?.Invoke();
    public static void FrameChanged(int frame) => OnFrameChanged?.Invoke(frame);
    public static void CamcorderLightChanged(bool isGreen) => OnCamcorderLightChanged?.Invoke(isGreen);
    public static void PlaybackEnded() => OnPlaybackEnded?.Invoke();
    public static void ZoneChanged(CamcorderZone newZone) => OnZoneChanged?.Invoke(newZone);

    // ── RECORDABLE EVENTS ────────────────────────────────────────────────────
    public static void RecordableEventStarted(string eventId) => OnRecordableEventStarted?.Invoke(eventId);
    public static void RecordableEventCompleted(string eventId) => OnRecordableEventCompleted?.Invoke(eventId);
    public static void RecordableEventInterrupted(string eventId) => OnRecordableEventInterrupted?.Invoke(eventId);
    public static void RecordableEventWatched(string id) => OnRecordableEventWatched?.Invoke(id);

    // ── CONFIRMATION ─────────────────────────────────────────────────────────
    /// <summary>
    /// Opens the confirmation panel with <paramref name="message"/>.
    /// <paramref name="onConfirm"/> fires when the player presses YES (E).
    /// <paramref name="onDecline"/> fires when the player presses NO (F) or walks away.
    /// </summary>
    public static void RequestConfirmation(string message, Action onConfirm, Action onDecline = null)
        => OnConfirmationRequested?.Invoke(message, onConfirm, onDecline);

    /// <summary>Closes the confirmation panel without firing any callback (e.g. player walked away).</summary>
    public static void CloseConfirmation() => OnConfirmationClosed?.Invoke();

    // ── READABLE ─────────────────────────────────────────────────────────────
    public static void ReadableOpened(UnityEngine.Sprite sprite, string text) => OnReadableOpened?.Invoke(sprite, text);
    public static void ReadableClosed() => OnReadableClosed?.Invoke();

    // ── PAUSE ────────────────────────────────────────────────────────────────
    public static void PauseChanged(bool paused) => OnPauseChanged?.Invoke(paused);
}