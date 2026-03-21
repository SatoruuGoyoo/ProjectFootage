using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public PlayerConfig config;
    [SerializeField] private float health;
    public float Health => health;

    // Only Testing purposes, remove later
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(5f); // Example damage value
        }
    }

    private void Start()
    {
        if (config != null)
        {
            health = config.MaxHealth;
            GameEvents.HealthChanged(health);
        }
    }

    // Take Damage ? maybe...?
    private void TakeDamage(float damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, config.MaxHealth);
        GameEvents.HealthChanged(health);

        if (health <= 0)
        {
            GameEvents.PlayerDied();
            Destroy(gameObject);
            Debug.Log("Player has died.");
        }
    }
}
