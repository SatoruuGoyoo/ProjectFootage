using UnityEngine;
using UnityEngine.InputSystem;

public class CamcorderAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float smoothSpeed = 8f;
    [SerializeField] private float layerTransitionSpeed = 5f;

    private PlayerInputActions actions;
    private Vector2 currentBlend;

    private static readonly int MoveX = Animator.StringToHash("CamMoveX");
    private static readonly int MoveY = Animator.StringToHash("CamMoveY");
    private const int UpperLayerIndex = 1;
    private const int LowerLayerIndex = 2;

    private bool camcorderEquipped = false;
    private float targetWeight = 0f;

    private void Awake() => actions = new PlayerInputActions();

    private void OnEnable()
    {
        actions.Exploration.Enable();
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    }

    private void OnPlayerModeChanged(PlayerMode mode)
    {
        camcorderEquipped = mode != PlayerMode.ExplorationMode;
        targetWeight = camcorderEquipped ? 1f : 0f;

        if (!camcorderEquipped)
        {
            currentBlend = Vector2.zero;
            animator.SetFloat(MoveX, 0f);
            animator.SetFloat(MoveY, 0f);
        }
    }

    private void Update()
    {
        
        float currentWeight = animator.GetLayerWeight(UpperLayerIndex);
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(UpperLayerIndex, newWeight);
        animator.SetLayerWeight(LowerLayerIndex, newWeight);

        if (!camcorderEquipped) return;

        Vector2 rawInput = actions.Exploration.Move.ReadValue<Vector2>();
        currentBlend = Vector2.Lerp(currentBlend, rawInput, Time.deltaTime * smoothSpeed);

        animator.SetFloat(MoveX, currentBlend.x);
        animator.SetFloat(MoveY, currentBlend.y);
    }
}