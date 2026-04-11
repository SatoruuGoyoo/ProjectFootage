using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public PlayerConfig config;

    public float Health { get; private set; }

    private void Start()
    {
        if (config == null) return;
        Health = config.MaxHealth;
        GameEvents.HealthChanged(Health);
    }

    // TODO: remove debug input before shipping
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            TakeDamage(5f);
    }

    public void TakeDamage(float damage)
    {
        Health = Mathf.Clamp(Health - damage, 0, config.MaxHealth);
        GameEvents.HealthChanged(Health);

        if (Health <= 0)
        {
            GameEvents.PlayerDied();
            Destroy(gameObject);
        }
    }
}