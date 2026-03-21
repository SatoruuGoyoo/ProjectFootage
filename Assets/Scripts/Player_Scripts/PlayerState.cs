using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public PlayerConfig config;
    private float health;

    private void Start()
    {
        if (config != null)
        {
            health = config.MaxHealth;
        }
    }

    // Take Damage ? maybe...?
}
