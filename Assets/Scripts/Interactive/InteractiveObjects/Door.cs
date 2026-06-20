using UnityEngine;
using FMODUnity;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool playerCanToggle = true;
    [SerializeField] private string lockedFeedback = "";

    [Header("Confirmation (shown once when player has the item)")]
    [SerializeField] private string confirmationText = "";

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Interaction")]
    [SerializeField] private float interactCooldown = 0.5f;

    [Header("Motion")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 120f;

    [Header("Audio")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference lockedSound;

    // ── State ─────────────────────────────────────────────────────────────────
    private bool isOpen;
    private bool manualLock;
    private bool _itemUsed;
    private bool _pendingConfirm;
    private float _interactCooldownTimer;
    private Quaternion closedRot;
    private Quaternion openRot;

    // ── Computed ──────────────────────────────────────────────────────────────

    private bool IsLocked => manualLock
        || (!string.IsNullOrEmpty(requiredItemId)
            && !_itemUsed
            && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId)));

    private bool NeedsConfirmation => !string.IsNullOrEmpty(requiredItemId)
        && !_itemUsed
        && !manualLock
        && ItemRegistry.Instance != null
        && ItemRegistry.Instance.Has(requiredItemId);

    // ── IInteractable ─────────────────────────────────────────────────────────

    public string PromptMessage => playerCanToggle || IsLocked || NeedsConfirmation ? "door" : "";
    public bool CanInteract => _interactCooldownTimer <= 0f && (IsLocked || playerCanToggle || NeedsConfirmation);
    public bool BlockMovement => false;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (pivot == null) pivot = transform;
        closedRot = pivot.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
        manualLock = startLocked;
        isOpen = startOpen;
        pivot.localRotation = isOpen ? openRot : closedRot;
    }

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    private void OnDisable() => GameEvents.OnConfirmationClosed -= OnConfirmationClosed;

    // ── IInteractable ─────────────────────────────────────────────────────────

    public void Interact()
    {
        if (_interactCooldownTimer > 0f) return;
        _interactCooldownTimer = interactCooldown;

        if (IsLocked)
        {
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, transform.position);
            GameEvents.FeedbackMessage(lockedFeedback);
            return;
        }

        if (NeedsConfirmation)
        {
            _pendingConfirm = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined);
            return;
        }

        if (!playerCanToggle) return;
        Toggle();
    }

    // ── Confirmation callbacks ────────────────────────────────────────────────

    private void OnConfirmed()
    {
        _pendingConfirm = false;
        _itemUsed = true;
        Open();
    }

    private void OnDeclined() => _pendingConfirm = false;
    private void OnConfirmationClosed() => _pendingConfirm = false;

    // ── Public API ────────────────────────────────────────────────────────────

    public void Toggle() { if (isOpen) Close(); else Open(); }

    public void Open()
    {
        if (IsLocked) return;
        isOpen = true;
        if (!openSound.IsNull) RuntimeManager.PlayOneShot(openSound, transform.position);
    }

    public void Close()
    {
        isOpen = false;
        if (!closeSound.IsNull) RuntimeManager.PlayOneShot(closeSound, transform.position);
    }

    public void Lock() => manualLock = true;
    public void Unlock() => manualLock = false;

    // ── Motion ────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (_interactCooldownTimer > 0f) _interactCooldownTimer -= Time.deltaTime;

        Quaternion target = isOpen ? openRot : closedRot;

        float step = speed * Time.deltaTime;


        pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, target, step);
    }
}