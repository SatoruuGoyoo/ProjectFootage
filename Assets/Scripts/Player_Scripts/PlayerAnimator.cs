using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    public Animator animator;
    public PlayerConfig config;

    private CharacterController characterController;
    private PlayerMotor motor;

    [Header("Triggers")]
    [SerializeField] private string pushTriggerName = "Push";


    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsWalkingBackHash = Animator.StringToHash("IsWalkingBack");
    private static readonly int IsSprintingHash = Animator.StringToHash("IsSprinting");
    private static readonly int IsTurningLeftHash = Animator.StringToHash("IsTurningLeft");
    private static readonly int IsTurningRightHash = Animator.StringToHash("IsTurningRight");
    private int _pushHash;

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
        _pushHash = Animator.StringToHash(pushTriggerName);
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        //animator.applyRootMotion = false;
    }
    public void TriggerPush() => PlayTrigger(_pushHash);
    public void TriggerPush(string overrideName)
    {
        if (string.IsNullOrEmpty(overrideName)) TriggerPush();
        else PlayTrigger(Animator.StringToHash(overrideName));
    }
    private void PlayTrigger(int hash)
    {
        if (animator == null) return;
        animator.ResetTrigger(hash);
        animator.SetTrigger(hash);
    }
    public void ClearPush()
    {
        if (animator == null) return;
        animator.ResetTrigger(_pushHash);
    }


    private void Update()
    {
        if (animator == null || config == null || motor == null) return;

        bool isBack = motor.IsMovingBackward;
        bool isSprint = motor.IsSprinting;
        animator.SetBool(IsWalkingHash, motor.HasInput && !isBack && !isSprint);
        animator.SetBool(IsWalkingBackHash, motor.HasInput && isBack);
        animator.SetBool(IsSprintingHash, isSprint);
        animator.SetBool(IsTurningLeftHash, motor.IsTurningLeft);
        animator.SetBool(IsTurningRightHash, motor.IsTurningRight);

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