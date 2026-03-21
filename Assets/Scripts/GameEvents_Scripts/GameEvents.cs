using System;

public static class GameEvents
{
    public static event Action OnPlayerDeath;
    public static event Action <float> OnHealthChanged;

    public static void PlayerDied()
    {
        OnPlayerDeath?.Invoke();
    }

    public static void HealthChanged(float newHealth)
    {
        OnHealthChanged?.Invoke(newHealth);
    }

}
    
