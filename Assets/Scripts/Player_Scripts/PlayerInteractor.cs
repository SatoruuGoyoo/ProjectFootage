using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float interactRange = 2f;
    [SerializeField] private LayerMask interactableMask = ~0;

    private PlayerInput input;
    private IInteractable current;
    private PlayerMode currentMode = PlayerMode.ExplorationMode;

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
        if (currentMode != PlayerMode.ExplorationMode) SetCurrent(null);
    }

    private void Update()
    {
        if (currentMode != PlayerMode.ExplorationMode) return;

        RefreshCurrent();

        if (current != null && input.Interact)
            current.Interact();
    }

    private void RefreshCurrent()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableMask);

        IInteractable best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IInteractable>(out var interactable)) continue;
            if (!interactable.CanInteract) continue;

            float d = (hit.transform.position - transform.position).sqrMagnitude;
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
        if (current == next) return;
        current = next;
        GameEvents.InteractPromptChanged(current != null ? current.PromptMessage : "");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}