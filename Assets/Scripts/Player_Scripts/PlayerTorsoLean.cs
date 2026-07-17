using UnityEngine;

public class PlayerTorsoLean : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Animator animator;
    [SerializeField] private CamcorderMotor camcorderMotor;

    [Header("Lean")]
    [SerializeField] private float leanFactor = 0.5f;
    [SerializeField] private float blendInSpeed = 4f;
    [SerializeField] private float blendOutSpeed = 4f;

    private Transform _torsoBone;
    private PlayerMode _currentMode = PlayerMode.ExplorationMode;
    private float _weight;

    private bool CamcorderUp => _currentMode == PlayerMode.CameraMode || _currentMode == PlayerMode.RecordingMode;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (camcorderMotor == null) camcorderMotor = GetComponentInChildren<CamcorderMotor>();

        if (animator != null)
        {
            _torsoBone = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (_torsoBone == null) _torsoBone = animator.GetBoneTransform(HumanBodyBones.Spine);
        }
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnModeChanged;

    private void OnModeChanged(PlayerMode newMode) => _currentMode = newMode;

    private void LateUpdate()
    {
        if (_torsoBone == null || camcorderMotor == null) return;

        float targetWeight = CamcorderUp ? 1f : 0f;
        float blendSpeed = targetWeight > _weight ? blendInSpeed : blendOutSpeed;
        _weight = Mathf.MoveTowards(_weight, targetWeight, blendSpeed * Time.deltaTime);

        if (_weight <= 0.001f) return;

        float leanAngle = camcorderMotor.CurrentTilt * leanFactor * _weight;
        _torsoBone.localRotation *= Quaternion.Euler(leanAngle, 0f, 0f);
    }
}