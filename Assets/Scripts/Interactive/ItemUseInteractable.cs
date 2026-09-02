using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

public class ItemUseInteractable : Interactable
{
    [Header("Item")]
    [SerializeField] private ItemData requiredItem;
    [SerializeField] private bool consumeItem = true;

    [Header("Prompts")]
    [SerializeField] private string promptWithItem = "usar";
    [SerializeField] private string promptWithoutItem = "";

    [Header("Feedback")]
    [SerializeField] private string missingItemMessage = "";
    [SerializeField] private string usedMessage = "";
    [SerializeField] private float feedbackDuration = -1f;

    [Header("Confirmation")]
    [SerializeField] private bool requiresConfirmation = true;
    [TextArea(2, 5)]
    [SerializeField] private string confirmationText = "";

    [Header("Settings")]
    [SerializeField] private bool oneTimeOnly = true;
    [SerializeField] private float interactCooldown = 0.4f;

    [Header("Audio")]
    [SerializeField] private EventReference useSound;
    [SerializeField] private EventReference missingItemSound;

    [Header("Events")]
    public UnityEvent OnUsed;
    public UnityEvent OnFirstUse;
    public UnityEvent OnMissingItem;

    private bool _used;
    private bool _usedOnce;
    private bool _pendingConfirmation;
    private float _cooldownTimer;

    private bool HasItem => requiredItem != null
        && ItemRegistry.Instance != null
        && ItemRegistry.Instance.Has(requiredItem);

    public override string PromptMessage => HasItem ? promptWithItem : promptWithoutItem;
    public override bool CanInteract => !_used && !_pendingConfirmation && _cooldownTimer <= 0f;
    public override bool BlockMovement => false;

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        _pendingConfirmation = false;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
    }

    public override void Interact()
    {
        if (!CanInteract) return;

        if (!HasItem)
        {
            ReportMissingItem();
            return;
        }

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined, uiPosition);
            return;
        }

        Use();
    }

    private void OnConfirmed()
    {
        _pendingConfirmation = false;
        Use();
    }

    private void OnDeclined() => _pendingConfirmation = false;

    private void OnConfirmationClosed() => _pendingConfirmation = false;

    private void ReportMissingItem()
    {
        _cooldownTimer = interactCooldown;

        if (!missingItemSound.IsNull)
            RuntimeManager.PlayOneShot(missingItemSound, transform.position);

        if (!string.IsNullOrEmpty(missingItemMessage))
            GameEvents.FeedbackMessage(missingItemMessage, uiPosition, feedbackDuration);

        OnMissingItem?.Invoke();
    }

    private void Use()
    {
        if (!HasItem) return;

        _cooldownTimer = interactCooldown;
        if (oneTimeOnly) _used = true;

        if (consumeItem) ItemRegistry.Instance.Remove(requiredItem);

        if (!useSound.IsNull)
            RuntimeManager.PlayOneShot(useSound, transform.position);

        if (!string.IsNullOrEmpty(usedMessage))
            GameEvents.FeedbackMessage(usedMessage, uiPosition, feedbackDuration);

        if (!_usedOnce)
        {
            _usedOnce = true;
            OnFirstUse?.Invoke();
        }

        OnUsed?.Invoke();
    }
}