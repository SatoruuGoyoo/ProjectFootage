using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Game/PlayerConfig")]
public class PlayerConfig : ScriptableObject
{
    [Header("Movement / Tank")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 120f;

    [Header("Movement / Modern")]
    [SerializeField] private float rotationSmoothSpeed = 8f;
    [SerializeField] private float modernMoveSpeed = 2.5f;

    [Header("Movement / Shared")]
    [SerializeField] private float sprintSpeed = 6f;

    [Header("Movement Feel")]
    [SerializeField] private float acceleration = 15f;
    [SerializeField] private float deceleration = 25f; 
    [SerializeField] private float gravityMultiplier = 2f;

    public float Deceleration => deceleration;

    [Header("Recording Modifiers")]
    [SerializeField] private float recordingSpeedTank = 1.5f;    
    [SerializeField] private float recordingSpeedModern = 1.5f;  
    [SerializeField] private float recordingRotationMultiplier = 0.5f;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;

    public float MoveSpeed => moveSpeed;
    public float TurnSpeed => turnSpeed;
    public float ModernMoveSpeed => modernMoveSpeed;
    public float SprintSpeed => sprintSpeed;
    public float RotationSmoothSpeed => rotationSmoothSpeed;
    public float Acceleration => acceleration;
    public float GravityMultiplier => gravityMultiplier;
    public float RecordingSpeedTank => recordingSpeedTank;
    public float RecordingSpeedModern => recordingSpeedModern;
    public float RecordingRotationMultiplier => recordingRotationMultiplier;
    public int MaxHealth => maxHealth;
}