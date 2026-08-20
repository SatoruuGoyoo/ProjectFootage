using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class Door : Interactable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool playerCanToggle = true;
    [SerializeField] private bool canClose = true;
    [SerializeField] private string lockedFeedback = "";
    [SerializeField] private float lockedFeedbackDuration = 3f;

    [Header("Confirmation (shown once when player has the item)")]
    [TextArea(2, 5)]
    [SerializeField] private string confirmationText = "";

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Interaction")]
    [SerializeField] private float interactCooldown = 0.5f;

    [Header("Motion")]
    [SerializeField] private Transform pivot;
    [SerializeField] private Transform door;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 120f;

    [Header("Audio")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference lockedSound;

    [Header("Events")]
    public UnityEvent OnFirstInteract;
    public UnityEvent OnFirstLockedInteract;
    public UnityEvent OnLockedInteract;
    public UnityEvent OnUnlockedWithItem;

    private bool isOpen;
    private bool manualLock;
    private bool _itemUsed;
    private bool _pendingConfirm;
    private bool _interactedOnce;
    private bool _lockedInteractedOnce;
    private bool _lockedFeedbackActive;
    private float _interactCooldownTimer;
    private Quaternion closedRot;
    private Quaternion openRot;
    private Quaternion doorClosedRot;
    private Quaternion doorOpenRot;

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

        SetupRotation(pivot, out closedRot, out openRot);
        if (door != null) SetupRotation(door, out doorClosedRot, out doorOpenRot);

        manualLock = startLocked;
        isOpen = startOpen;

        pivot.localRotation = isOpen ? openRot : closedRot;
        if (door != null) door.localRotation = isOpen ? doorOpenRot : doorClosedRot;
    }

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    private void OnDisable() => GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
    private void SetupRotation(Transform t, out Quaternion closed, out Quaternion open)
    {
        if (startOpen)
        {
            open = t.localRotation;
            closed = open * Quaternion.Euler(0f, -openAngle, 0f);
        }
        else
        {
            closed = t.localRotation;
            open = closed * Quaternion.Euler(0f, openAngle, 0f);
        }
    }
    public override void Interact()
    {
        if (_interactCooldownTimer > 0f) return;

        if (!_interactedOnce)
        {
            _interactedOnce = true;
            OnFirstInteract?.Invoke();
        }

        if (IsLocked)
        {
            if (_lockedFeedbackActive) return;
            _interactCooldownTimer = interactCooldown;
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, transform.position);
            StartCoroutine(LockedFeedbackRoutine());
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

    private IEnumerator LockedFeedbackRoutine()
    {
        _lockedFeedbackActive = true;
        GameEvents.FeedbackMessage(lockedFeedback, uiPosition, lockedFeedbackDuration);

        yield return new WaitForSeconds(lockedFeedbackDuration);

        OnLockedInteract?.Invoke();
        if (!_lockedInteractedOnce)
        {
            _lockedInteractedOnce = true;
            OnFirstLockedInteract?.Invoke();
        }

        _lockedFeedbackActive = false;
    }

    private void OnConfirmed()
    {
        _pendingConfirm = false;
        _itemUsed = true;
        _interactCooldownTimer = interactCooldown;
        OnUnlockedWithItem?.Invoke();
        Open();
    }

    private void OnDeclined()
    {
        _pendingConfirm = false;
    }

    private void OnConfirmationClosed() => _pendingConfirm = false;

    public void Toggle() { if (isOpen) Close(); else Open(); }

    public void Open()
    {
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

        if (door != null)
        {
            Quaternion doorTarget = isOpen ? doorOpenRot : doorClosedRot;
            door.localRotation = Quaternion.RotateTowards(door.localRotation, doorTarget, speed * Time.deltaTime);
        }
    }
}