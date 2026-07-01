using UnityEngine;
using FMODUnity;

public class Door : Interactable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool playerCanToggle = true;
    [SerializeField] private bool canClose = true;
    [SerializeField] private string lockedFeedback = "";

    [Header("Confirmation (shown once when player has the item)")]
    [TextArea(2, 5)]
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

    private bool isOpen;
    private bool manualLock;
    private bool _itemUsed;
    private bool _pendingConfirm;
    private float _interactCooldownTimer;
    private Quaternion closedRot;
    private Quaternion openRot;

    private bool IsLocked => manualLock
        || (!string.IsNullOrEmpty(requiredItemId)
            && !_itemUsed
            && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId)));

    private bool NeedsConfirmation => !string.IsNullOrEmpty(requiredItemId)
        && !_itemUsed
        && !manualLock
        && ItemRegistry.Instance != null
        && ItemRegistry.Instance.Has(requiredItemId);

    private bool CanToggle => playerCanToggle && (!isOpen || canClose);

    public override string PromptMessage => (CanToggle || IsLocked || NeedsConfirmation) ? "door" : "";
    public override bool CanInteract => _interactCooldownTimer <= 0f && (IsLocked || NeedsConfirmation || CanToggle);
    public override bool BlockMovement => false;

    private void Awake()
    {
        if (pivot == null) pivot = transform;

        if (startOpen)
        {
            openRot = pivot.localRotation;
            closedRot = openRot * Quaternion.Euler(0f, -openAngle, 0f);
        }
        else
        {
            closedRot = pivot.localRotation;
            openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
        }

        manualLock = startLocked;
        isOpen = startOpen;
        pivot.localRotation = isOpen ? openRot : closedRot;
    }

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    private void OnDisable() => GameEvents.OnConfirmationClosed -= OnConfirmationClosed;

    public override void Interact()
    {
        if (_interactCooldownTimer > 0f) return;

        if (IsLocked)
        {
            _interactCooldownTimer = interactCooldown;
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, transform.position);
            GameEvents.FeedbackMessage(lockedFeedback, uiPosition);
            return;
        }

        if (NeedsConfirmation)
        {
            if (_pendingConfirm) return;
            _pendingConfirm = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined, uiPosition);
            return;
        }

        if (!CanToggle) return;
        _interactCooldownTimer = interactCooldown;
        Toggle();
    }

    private void OnConfirmed()
    {
        _pendingConfirm = false;
        _itemUsed = true;
        _interactCooldownTimer = interactCooldown;
        Open();
    }

    private void OnDeclined()
    {
        _pendingConfirm = false;
        _interactCooldownTimer = interactCooldown;
    }

    private void OnConfirmationClosed() => _pendingConfirm = false;

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

    private void Update()
    {
        if (_interactCooldownTimer > 0f) _interactCooldownTimer -= Time.deltaTime;

        Quaternion target = isOpen ? openRot : closedRot;
        pivot.localRotation = Quaternion.RotateTowards(pivot.localRotation, target, speed * Time.deltaTime);
    }
}