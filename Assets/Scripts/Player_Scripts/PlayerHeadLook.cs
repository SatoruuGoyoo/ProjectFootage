using UnityEngine;

public class PlayerHeadLook : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Vector3 boxSize = new Vector3(6f, 3f, 6f);
    [SerializeField] private Vector3 boxCenter = Vector3.zero;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private float refreshDelay = 0.15f;
    [Tooltip("El nuevo target tiene que estar a esta fracción de la distancia actual (o menos) para reemplazarlo. Evita que titile entre dos interactuables a distancias parecidas.")]
    [Range(0.1f, 1f)][SerializeField] private float switchMargin = 0.85f;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask occluderMask = 0;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1.4f, 0f);

    [Header("Interaction")]
    [Tooltip("Mantiene la mirada en el objeto mientras dura la interacción, en vez de volver a reposo.")]
    [SerializeField] private bool holdTargetWhileInteracting = true;

    [Header("Head Aim")]
    [SerializeField] private Animator animator;
    [SerializeField] private float maxYawAngle = 60f;
    [SerializeField] private float maxPitchAngle = 30f;
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float blendInSpeed = 3f;
    [SerializeField] private float blendOutSpeed = 4f;

    private Transform _headBone;
    private Transform _currentTargetTransform;
    private PlayerMode _currentMode = PlayerMode.ExplorationMode;
    private Quaternion _currentLookRotation;
    private float _weight;
    private float _refreshTimer;

    private Collider[] _hits = new Collider[32];

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (animator != null) _headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        _currentLookRotation = _headBone != null ? _headBone.rotation : transform.rotation;
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnModeChanged;

    private void OnModeChanged(PlayerMode newMode)
    {
        bool returningFromInteraction = _currentMode == PlayerMode.InteractionMode
            && newMode == PlayerMode.ExplorationMode;

        _currentMode = newMode;

        if (returningFromInteraction) _refreshTimer = 0f;
        else if (newMode == PlayerMode.InteractionMode && !holdTargetWhileInteracting)
            _currentTargetTransform = null;
    }

    private void Update()
    {
        if (_currentMode != PlayerMode.ExplorationMode) return;

        _refreshTimer -= Time.deltaTime;
        if (_refreshTimer <= 0f)
        {
            RefreshTarget();
            _refreshTimer = refreshDelay;
        }
    }

    private void RefreshTarget()
    {
        Vector3 center = transform.TransformPoint(boxCenter);
        int hitCount = Physics.OverlapBoxNonAlloc(center, boxSize * 0.5f, _hits, transform.rotation, interactableMask);

        Vector3 eye = transform.TransformPoint(eyeOffset);

        Transform bestTransform = null;
        float bestDist = float.MaxValue;
        bool currentStillValid = false;
        float currentDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var interactable = _hits[i].GetComponentInParent<IInteractable>();
            if (interactable == null) continue;
            if (!interactable.CanInteract && !interactable.IsActive) continue;
            if (InteractionSight.IsBlocked(eye, _hits[i], interactable, occluderMask)) continue;

            float d = (_hits[i].transform.position - transform.position).sqrMagnitude;

            if (_hits[i].transform == _currentTargetTransform)
            {
                currentStillValid = true;
                currentDist = d;
            }

            if (d < bestDist)
            {
                bestDist = d;
                bestTransform = _hits[i].transform;
            }
        }

        if (currentStillValid && bestTransform != _currentTargetTransform &&
            bestDist > currentDist * switchMargin * switchMargin)
        {
            return;
        }

        _currentTargetTransform = bestTransform;
    }

    private bool HasValidTarget()
    {
        if (_currentTargetTransform == null) return false;
        if (!_currentTargetTransform.gameObject.activeInHierarchy) return false;
        if (_currentMode != PlayerMode.ExplorationMode && !holdTargetWhileInteracting) return false;
        return true;
    }

    private void LateUpdate()
    {
        if (_headBone == null) return;

        bool hasTarget = HasValidTarget();
        float targetWeight = 0f;

        if (hasTarget)
        {
            Vector3 desiredDir = (_currentTargetTransform.position - _headBone.position).normalized;
            Vector3 local = transform.InverseTransformDirection(desiredDir);

            float yaw = Mathf.Clamp(Mathf.Atan2(local.x, local.z) * Mathf.Rad2Deg, -maxYawAngle, maxYawAngle);
            float pitch = Mathf.Clamp(Mathf.Asin(Mathf.Clamp(local.y, -1f, 1f)) * Mathf.Rad2Deg, -maxPitchAngle, maxPitchAngle);

            float yawRad = yaw * Mathf.Deg2Rad;
            float pitchRad = pitch * Mathf.Deg2Rad;
            Vector3 clampedLocal = new Vector3(
                Mathf.Sin(yawRad) * Mathf.Cos(pitchRad),
                Mathf.Sin(pitchRad),
                Mathf.Cos(yawRad) * Mathf.Cos(pitchRad));

            Vector3 clampedWorldDir = transform.TransformDirection(clampedLocal);
            Quaternion desiredRot = Quaternion.LookRotation(clampedWorldDir, Vector3.up);

            bool isNewEngagement = _weight <= 0.001f;
            _currentLookRotation = isNewEngagement
                ? desiredRot
                : Quaternion.RotateTowards(_currentLookRotation, desiredRot, turnSpeed * Time.deltaTime);

            targetWeight = 1f;
        }

        float blendSpeed = targetWeight > _weight ? blendInSpeed : blendOutSpeed;
        _weight = Mathf.MoveTowards(_weight, targetWeight, blendSpeed * Time.deltaTime);

        if (_weight > 0.001f)
            _headBone.rotation = Quaternion.Slerp(_headBone.rotation, _currentLookRotation, _weight);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.35f);
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCenter), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}