using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Vector3 boxSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 boxCenter = Vector3.zero;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private float refreshDelay = 0.1f;

    private InputAction _interactAction;
    private InputAction _cancelAction;
    private IInteractable current;
    private PlayerMode currentMode = PlayerMode.ExplorationMode;
    private bool _suppressInteractThisFrame;

    private Collider[] hits = new Collider[32];
    private float refreshTimer;
    private string lastPrompt = "";
    private Sprite lastIcon;
    private bool lastActive;

    private void Start()
    {
        _interactAction = PlayerInput.Actions.Exploration.Interact;
        _cancelAction = PlayerInput.Actions.UI.Cancel;
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnModeChanged;

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnModeChanged;
        SetCurrent(null);
    }

    private void OnModeChanged(PlayerMode newMode)
    {
        bool returningFromInteraction = currentMode == PlayerMode.InteractionMode && newMode == PlayerMode.ExplorationMode;
        currentMode = newMode;

        if (!returningFromInteraction)
        {
            if (newMode == PlayerMode.ExplorationMode) SetCurrent(null);
            return;
        }

        _suppressInteractThisFrame = true;
        current = null;
        lastActive = false;
        lastPrompt = "";
        lastIcon = null;
        RefreshCurrent();
        refreshTimer = refreshDelay;
    }

    private void Update()
    {
        if (_suppressInteractThisFrame)
        {
            _suppressInteractThisFrame = false;
            return;
        }

        if (currentMode == PlayerMode.ExplorationMode)
        {
            UpdateExploration();
        }
        else if (currentMode == PlayerMode.InteractionMode && current != null && current.BlockMovement)
        {
            if (_interactAction.WasPressedThisFrame()) current.Interact();
        }
    }

    private void UpdateExploration()
    {
        refreshTimer -= Time.deltaTime;
        if (refreshTimer <= 0f)
        {
            RefreshCurrent();
            refreshTimer = refreshDelay;
        }

        if (current != null && _interactAction.WasPressedThisFrame())
        {
            current.Interact();
            RefreshCurrent();
        }

        if (current != null && current.IsActive && !current.KeepProximityKeyWhenActive && _cancelAction.WasPressedThisFrame())
        {
            current.Interact();
            RefreshCurrent();
        }
    }

    private void RefreshCurrent()
    {
        Vector3 center = transform.TransformPoint(boxCenter);
        int hitCount = Physics.OverlapBoxNonAlloc(center, boxSize * 0.5f, hits, transform.rotation, interactableMask);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            if (!hits[i].TryGetComponent<IInteractable>(out var interactable)) continue;
            if (!interactable.CanInteract) continue;

            float d = (hits[i].transform.position - transform.position).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = interactable;
            }
        }

        SetCurrent(best);
    }

    private void SetCurrent(IInteractable next)
    {
        bool nextActive = next != null && next.IsActive;
        string prompt = next != null ? next.PromptMessage : "";
        Sprite icon = next != null ? (nextActive ? next.ActiveIcon : next.PromptIcon) : null;

        if (next == current && nextActive == lastActive && prompt == lastPrompt && icon == lastIcon)
            return;

        bool wasActive = lastActive;
        current = next;
        lastActive = nextActive;
        lastPrompt = prompt;
        lastIcon = icon;

        if (next == null)
        {
            GameEvents.InteractPromptDeactivated();
            GameEvents.InteractPromptChanged("", null);
            return;
        }

        if (nextActive)
        {
            GameEvents.InteractPromptActivated(prompt, icon, next.KeepProximityKeyWhenActive);
            return;
        }

        if (wasActive) GameEvents.InteractPromptDeactivated();
        GameEvents.InteractPromptChanged(prompt, icon);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(boxCenter), transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}