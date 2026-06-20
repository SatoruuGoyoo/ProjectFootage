using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableMask = ~0;
    [SerializeField] private float refreshDelay = 0.1f;

    private PlayerInput input;
    private IInteractable current;
    private PlayerMode currentMode = PlayerMode.ExplorationMode;

    private Collider[] hits = new Collider[32];
    private float refreshTimer;
    private string lastPrompt = "";

    private void Awake() => input = GetComponent<PlayerInput>();

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnModeChanged;

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnModeChanged;
        SetCurrent(null);
    }

    private void OnModeChanged(PlayerMode newMode)
    {
        currentMode = newMode;
        if (newMode == PlayerMode.ExplorationMode)
            SetCurrent(null);
    }

    private void Update()
    {
        if (currentMode == PlayerMode.ExplorationMode)
        {
            UpdateExploration();
        }
        else if (currentMode == PlayerMode.InteractionMode && current != null && current.BlockMovement)
        {
            if (input.Interact) current.Interact();
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

        if (current != null && input.Interact)
            current.Interact();
    }

    private void RefreshCurrent()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, interactRange, hits, interactableMask);

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
        current = next;
        string prompt = current != null ? current.PromptMessage : "";
        if (prompt == lastPrompt) return;
        lastPrompt = prompt;
        GameEvents.InteractPromptChanged(prompt);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}