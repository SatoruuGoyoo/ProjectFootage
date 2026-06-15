using UnityEngine;

public class CamcorderAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float layerTransitionSpeed = 5f;

    private const int UpperLayerIndex = 1;
    private bool camcorderEquipped = false;
    private float targetWeight = 0f;

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;

    private void OnPlayerModeChanged(PlayerMode mode)
    {
        camcorderEquipped = mode == PlayerMode.CameraMode
                         || mode == PlayerMode.RecordingMode
                         || mode == PlayerMode.MenuCameraMode;
        targetWeight = camcorderEquipped ? 1f : 0f;
    }

    private void Update()
    {
        float currentWeight = animator.GetLayerWeight(UpperLayerIndex);
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * layerTransitionSpeed);
        animator.SetLayerWeight(UpperLayerIndex, newWeight);
    }
}