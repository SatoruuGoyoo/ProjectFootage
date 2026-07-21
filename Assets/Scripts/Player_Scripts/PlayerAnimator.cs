using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerConfig config;

    private CharacterController characterController;
    private PlayerMotor motor;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsWalkingBackHash = Animator.StringToHash("IsWalkingBack");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private PlayerMode currentMode = PlayerMode.ExplorationMode;

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    private void OnPlayerModeChanged(PlayerMode mode) => currentMode = mode;

    private static readonly int MoveXHash = Animator.StringToHash("MoveX");
    private static readonly int MoveYHash = Animator.StringToHash("MoveY");

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

        bool isBack = motor.IsMovingBackward;
        bool isSprint = motor.IsSprinting;
        animator.SetBool(IsWalkingHash, motor.HasInput && !isBack && !isSprint);
        animator.SetBool(IsWalkingBackHash, motor.HasInput && isBack);
        animator.SetBool(IsSprintingHash, isSprint);

        if (currentMode == PlayerMode.ExplorationMode)
        {
            Vector3 horizontalVel = characterController.velocity;
            horizontalVel.y = 0f;
            float normalizedSpeed = Mathf.Clamp01(horizontalVel.magnitude / config.MoveSpeed);
            animator.speed = motor.HasInput ? Mathf.Max(normalizedSpeed, 0.3f) : 1f;
        }
        else
        {
            animator.speed = 1f;
        }
    }
}