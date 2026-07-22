using System;

public static class GameEvents
{
    // Player
    public static event Action OnPlayerDeath;
    public static event Action<float> OnHealthChanged;

    // Interaction
    public static event Action<string, UnityEngine.Sprite> OnInteractPromptChanged;

    // Interact Prompt State
    public static event Action<string, UnityEngine.Sprite> OnInteractPromptActivated;
    public static event Action OnInteractPromptDeactivated;

    // Items
    public static event Action<string> OnItemCollected;
    public static event Action<string, UIPositioner.ScreenPosition, float> OnFeedbackMessage;

    // Entity
    public static event Action<string, UIPositioner.ScreenPosition, float> OnEntityFeedbackMessage;

    // Tutorial
    public static event Action<string, UIPositioner.ScreenPosition> OnTutorialPromptShown;
    public static event Action OnTutorialPromptHidden;

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
    public static event Action<RecordingSession> OnRecordingDiscarded;

    // Confirmation
    public static event Action<string, Action, Action, UIPositioner.ScreenPosition> OnConfirmationRequested;
    public static event Action OnConfirmationClosed;

    // Readable
    public static event Action<UnityEngine.Sprite, string[], UIPositioner.ScreenPosition, Action> OnReadableOpened;
    public static event Action OnReadableClosed;



    // ── PLAYER ───────────────────────────────────────────────────────────────
    public static void PlayerDied() => OnPlayerDeath?.Invoke();
    public static void HealthChanged(float newHealth) => OnHealthChanged?.Invoke(newHealth);
    public static void PlayerModeChanged(PlayerMode newMode) => OnPlayerModeChanged?.Invoke(newMode);
    public static void ControllerSchemeChanged(ControlScheme scheme) => OnControllerSchemeChanged?.Invoke(scheme);

    // ── INTERACTION ──────────────────────────────────────────────────────────
    public static void InteractPromptChanged(string newPrompt, UnityEngine.Sprite icon = null) => OnInteractPromptChanged?.Invoke(newPrompt, icon);
    public static void InteractPromptActivated(string promptType, UnityEngine.Sprite icon = null) => OnInteractPromptActivated?.Invoke(promptType, icon);
    public static void InteractPromptDeactivated() => OnInteractPromptDeactivated?.Invoke();

    // ── ITEMS ────────────────────────────────────────────────────────────────
    public static void ItemCollected(string itemId) => OnItemCollected?.Invoke(itemId);

    public static void FeedbackMessage(string message, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter, float duration = -1f)
        => OnFeedbackMessage?.Invoke(message, position, duration);

    // ── ENTITY ───────────────────────────────────────────────────────────────
    public static void EntityFeedbackMessage(string message, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter, float duration = -1f)
        => OnEntityFeedbackMessage?.Invoke(message, position, duration);

    // ── TUTORIAL ─────────────────────────────────────────────────────────────
    public static void TutorialPromptShown(string message, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter)
        => OnTutorialPromptShown?.Invoke(message, position);
    public static void TutorialPromptHidden() => OnTutorialPromptHidden?.Invoke();

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
    public static void RecordingDiscarded(RecordingSession session) => OnRecordingDiscarded?.Invoke(session);

    // ── CONFIRMATION ─────────────────────────────────────────────────────────
    public static void RequestConfirmation(string message, Action onConfirm, Action onDecline = null, UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.MiddleCenter)
        => OnConfirmationRequested?.Invoke(message, onConfirm, onDecline, position);
    public static void CloseConfirmation() => OnConfirmationClosed?.Invoke();

    // ── READABLE ─────────────────────────────────────────────────────────────
    public static void ReadableOpened(UnityEngine.Sprite sprite, string[] pages, UIPositioner.ScreenPosition position, Action onCloseRequested)
    => OnReadableOpened?.Invoke(sprite, pages, position, onCloseRequested);
    public static void ReadableClosed() => OnReadableClosed?.Invoke();

    // ── PAUSE ────────────────────────────────────────────────────────────────
    public static void PauseChanged(bool paused) => OnPauseChanged?.Invoke(paused);
}