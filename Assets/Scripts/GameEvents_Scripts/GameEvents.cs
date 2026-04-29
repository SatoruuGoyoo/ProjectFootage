using System;

public static class GameEvents
{
    // Player
    public static event Action OnPlayerDeath;
    public static event Action<float> OnHealthChanged;

    // Player Mode
    public static event Action<PlayerMode> OnPlayerModeChanged;

    // Control Scheme
    public static event Action<ControlScheme> OnControllerSchemeChanged;

    // Camcorder
    public static event Action OnRecordingStarted;
    public static event Action OnRecordingStopped;
    public static event Action<int> OnFrameChanged;
    public static event Action<bool> OnCamcorderLightChanged;
    public static event Action OnPlaybackEnded;

    // Puzzle
    public static event Action OnClockSolved;
    public static event Action<int> OnIterationChanged;
    public static event Action<string> OnPuzzleCompleted;


    // PLAYER
    public static void PlayerDied() => OnPlayerDeath?.Invoke();
    public static void HealthChanged(float newHealth) => OnHealthChanged?.Invoke(newHealth);
    public static void PlayerModeChanged(PlayerMode newMode) => OnPlayerModeChanged?.Invoke(newMode);
    public static void ControllerSchemeChanged(ControlScheme scheme) => OnControllerSchemeChanged?.Invoke(scheme);

    // CAMCORDER
    public static void RecordingStarted() => OnRecordingStarted?.Invoke();
    public static void RecordingStopped() => OnRecordingStopped?.Invoke();
    public static void FrameChanged(int frame) => OnFrameChanged?.Invoke(frame);
    public static void CamcorderLightChanged(bool isGreen) => OnCamcorderLightChanged?.Invoke(isGreen);
    public static void PlaybackEnded() => OnPlaybackEnded?.Invoke();

    // PUZZLE
    public static void ClockSolved() => OnClockSolved?.Invoke();
    public static void IterationChanged(int iteration) => OnIterationChanged?.Invoke(iteration);
    public static void PuzzleCompleted(string puzzleId) => OnPuzzleCompleted?.Invoke(puzzleId);
}