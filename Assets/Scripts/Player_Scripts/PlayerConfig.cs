using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement — Tank")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 120f;

    [Header("Movement — Modern")]
    [SerializeField] private float rotationSmoothSpeed = 8f;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;

    public float MoveSpeed => moveSpeed;
    public float TurnSpeed => turnSpeed;
    public float RotationSmoothSpeed => rotationSmoothSpeed;
    public int MaxHealth => maxHealth;
}