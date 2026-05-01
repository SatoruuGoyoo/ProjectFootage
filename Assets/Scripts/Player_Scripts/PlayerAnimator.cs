using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerConfig config;

    private CharacterController characterController;
    private PlayerMotor motor;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        motor = GetComponent<PlayerMotor>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        animator.applyRootMotion = false;
    }

    private void Update()
    {
        if (animator == null || config == null || motor == null) return;

        // Transición inmediata basada en input, no en velocidad
        animator.SetBool(IsWalkingHash, motor.HasInput);

        // Velocidad de reproducción sigue a la velocidad real → se ve natural
        Vector3 horizontalVel = characterController.velocity;
        horizontalVel.y = 0f;
        float normalizedSpeed = Mathf.Clamp01(horizontalVel.magnitude / config.MoveSpeed);
        animator.speed = motor.HasInput ? Mathf.Max(normalizedSpeed, 0.3f) : 1f;
    }
}