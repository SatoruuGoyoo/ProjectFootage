using System;

public static class GameEvents
{
    // Events for player health and death
    public static event Action OnPlayerDeath;
    public static event Action <float> OnHealthChanged;
    public static event Action<int> OnFrameChanged;

    // Events for PlayerMode
    public static event Action <PlayerMode>OnPlayerModeChanged;

    public static event Action <ControlScheme> OnControllerSchemeChanged;

    public static event Action OnClockSolved;

    public static event Action<int> OnIterationChanged;

    public static event Action<string> OnPuzzleCompleted;

    public static void IterationChanged(int newIteration)
    {
        OnIterationChanged?.Invoke(newIteration);
    }

    /// <param name="puzzleId">ID único del puzzle que se completó (ej: "DoorPuzzle", "ClockRoom").</param>
    public static void PuzzleCompleted(string puzzleId)
    {
        OnPuzzleCompleted?.Invoke(puzzleId);
    }

    public static void ClockSolved()
    {
        OnClockSolved?.Invoke();
    }

    public static void ControllerSchemeChanged(ControlScheme newScheme)
    {
        OnControllerSchemeChanged?.Invoke(newScheme);
    }


    public static void PlayerModeChanged(PlayerMode newMode)
    {
        OnPlayerModeChanged?.Invoke(newMode);
    }


    public static void FrameChanged(int frame)
    {
        OnFrameChanged?.Invoke(frame);
    }


    // Methods for Player health and death
    public static void PlayerDied()
    {
        OnPlayerDeath?.Invoke();
    }

    public static void HealthChanged(float newHealth)
    {
        OnHealthChanged?.Invoke(newHealth);
    }

}
    
