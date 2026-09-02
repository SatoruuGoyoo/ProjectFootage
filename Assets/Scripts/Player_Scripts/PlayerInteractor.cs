using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Vector3 boxSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 boxCenter = Vector3.zero;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private float refreshDelay = 0.1f;

    [Header("Prompt Range")]
    [Tooltip("Mientras no haya nada al alcance, el prompt sigue al objetivo del head look, atenuado por distancia.")]
    [SerializeField] private PlayerHeadLook headLook;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask occluderMask = 0;
    [SerializeField] private Vector3 eyeOffset = new Vector3(0f, 1.4f, 0f);

    private readonly Collider[] _hits = new Collider[32];

    private InputAction _interactAction;
    private InputAction _cancelAction;

    private IInteractable _current;
    private IInteractable _promptTarget;
    private PlayerMode _mode = PlayerMode.ExplorationMode;
    private float _refreshTimer;
    private bool _skipInputThisFrame;

    private InteractPrompt _published;
    private bool _promptVisible;

    private void Awake()
    {
        if (headLook == null) headLook = GetComponentInParent<PlayerHeadLook>();
        if (headLook == null) headLook = GetComponentInChildren<PlayerHeadLook>();
        if (headLook == null) headLook = FindObjectOfType<PlayerHeadLook>();

        if (headLook == null)
            Debug.LogWarning("[PlayerInteractor] Sin PlayerHeadLook: el prompt sólo va a aparecer dentro del rango de interacción.", this);
    }

    private void Start()
    {
        _interactAction = PlayerInput.Actions.Exploration.Interact;
        _cancelAction = PlayerInput.Actions.UI.Cancel;
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnModeChanged;

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnModeChanged;
        _current = null;
        _promptTarget = null;
        PublishPrompt();
    }

    private void Update()
    {
        if (_mode == PlayerMode.ExplorationMode)
        {
            _refreshTimer -= Time.deltaTime;
            if (_refreshTimer <= 0f)
            {
                RefreshCurrent();
                _refreshTimer = refreshDelay;
            }
        }

        if (_skipInputThisFrame) _skipInputThisFrame = false;
        else HandleInput();

        PublishPrompt();
    }

    private void HandleInput()
    {
        if (!HasCurrent()) return;
        if (_mode == PlayerMode.InteractionMode && !_current.BlockMovement) return;

        if (_interactAction.WasPressedThisFrame())
        {
            if (!_current.CanInteract) return;
            _current.Interact();
        }
        else if (_cancelAction.WasPressedThisFrame())
        {
            if (!_current.IsActive) return;
            _current.Cancel();
        }
        else
        {
            return;
        }

        if (_mode == PlayerMode.ExplorationMode) RefreshCurrent();
    }

    private void OnModeChanged(PlayerMode newMode)
    {
        bool returningFromInteraction = _mode == PlayerMode.InteractionMode && newMode == PlayerMode.ExplorationMode;
        _mode = newMode;

        if (!returningFromInteraction) return;

        _skipInputThisFrame = true;
        _refreshTimer = 0f;
    }

    private void RefreshCurrent()
    {
        _current = FindNearest(boxSize);
        _promptTarget = _current ?? (headLook != null ? headLook.CurrentTarget : null);
    }

    private IInteractable FindNearest(Vector3 size)
    {
        Vector3 center = transform.TransformPoint(boxCenter);
        int hitCount = Physics.OverlapBoxNonAlloc(center, size * 0.5f, _hits, transform.rotation, interactableMask, QueryTriggerInteraction.Collide);

        Vector3 eye = transform.TransformPoint(eyeOffset);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            var target = _hits[i].GetComponentInParent<IInteractable>();
            if (target == null) continue;
            if (!target.CanInteract && !target.IsActive) continue;
            if (InteractionSight.IsBlocked(eye, _hits[i], target, occluderMask)) continue;

            if (target.IsActive) return target;

            float d = (_hits[i].transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = target;
            }
        }

        return best;
    }

    private void PublishPrompt()
    {
        if (!HasCurrent()) _current = null;
        if (!IsAlive(_promptTarget)) _promptTarget = null;

        IInteractable target = _promptTarget ?? _current;
        InteractPrompt prompt = InteractPrompt.From(target, target != null && ReferenceEquals(target, _current));
        bool visible = prompt.IsVisible;

        if (visible == _promptVisible && prompt.Equals(_published)) return;

        _promptVisible = visible;
        _published = prompt;

        if (visible) GameEvents.InteractPromptShown(prompt);
        else GameEvents.InteractPromptHidden();
    }

    private bool HasCurrent() => IsAlive(_current);

    private static bool IsAlive(IInteractable target)
    {
        if (target == null) return false;
        return !(target is Object obj) || obj != null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCenter), transform.rotation, Vector3.one);

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}