using System;

public static class GameEvents
{
    // Events for player health and death
    public static event Action OnPlayerDeath;
    public static event Action <float> OnHealthChanged;

    // Events for PlayerMode
    public static event Action <PlayerMode>OnPlayerModeChanged;


    public static void PlayerModeChanged(PlayerMode newMode)
    {
        OnPlayerModeChanged?.Invoke(newMode);
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
    
