using UnityEngine;

public class RootMotionYRelay : MonoBehaviour
{
    public Animator animator;
    public PlayerMotor motor;

    [SerializeField] float maxBobOffset = 0.1f;
    [SerializeField] float maxDeltaPerFrame = 0.02f;
    [SerializeField] float returnToRestSpeed = 6f;

    Vector3 basePosition;
    float currentOffsetY;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (motor == null) motor = GetComponentInParent<PlayerMotor>();
        basePosition = transform.localPosition;
    }

    void OnAnimatorMove()
    {
        float deltaY = Mathf.Clamp(animator.deltaPosition.y, -maxDeltaPerFrame, maxDeltaPerFrame);
        currentOffsetY += deltaY;
        currentOffsetY = Mathf.Clamp(currentOffsetY, -maxBobOffset, maxBobOffset);

        if (!motor.HasInput)
            currentOffsetY = Mathf.MoveTowards(currentOffsetY, 0f, returnToRestSpeed * Time.deltaTime);

        transform.localPosition = new Vector3(basePosition.x, basePosition.y + currentOffsetY, basePosition.z);
        transform.localRotation = Quaternion.identity;
    }
}