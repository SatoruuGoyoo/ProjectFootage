using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 120f;

    [Header("Modern")]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    public float RotationSmoothSpeed => rotationSmoothSpeed;

    [Header("Player Stats")]
    [SerializeField] private int maxHealth = 100;

    // Properties to access the private fields
    public float MoveSpeed => moveSpeed;
    public float TurnSpeed => turnSpeed;
    public int MaxHealth => maxHealth;


}
