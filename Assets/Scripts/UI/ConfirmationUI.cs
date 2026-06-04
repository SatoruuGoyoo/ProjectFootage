using TMPro;
using UnityEngine;

public class ConfirmationUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;

    [Header("Labels")]
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private TMP_Text yesLabel;
    [SerializeField] private TMP_Text noLabel;

    [Header("Default button labels")]
    [SerializeField] private string yesText = "[E] Sí";
    [SerializeField] private string noText = "[F] No";

    // ── Runtime state ─────────────────────────────────────────────────────────
    private bool _isOpen;
    private System.Action _onConfirm;
    private System.Action _onDecline;

    // Reference to the player input — set in Awake via FindAnyObjectByType.
    // If you have a ServiceLocator/DI, swap this out.
    private PlayerInput _input;

    // ── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _input = FindAnyObjectByType<PlayerInput>();

        if (yesLabel != null) yesLabel.SetText(yesText);
        if (noLabel != null) noLabel.SetText(noText);

        ForceHide();
    }

    private void OnEnable()
    {
        GameEvents.OnConfirmationRequested += OnRequested;
        GameEvents.OnConfirmationClosed += OnClosedExternally;
    }

    private void OnDisable()
    {
        GameEvents.OnConfirmationRequested -= OnRequested;
        GameEvents.OnConfirmationClosed -= OnClosedExternally;
    }

    private void Update()
    {
        if (!_isOpen || _input == null) return;

        if (_input.Interact) { Confirm(); return; }
        if (_input.Decline) { Decline(); return; }
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnRequested(string message, System.Action onConfirm, System.Action onDecline)
    {
        _onConfirm = onConfirm;
        _onDecline = onDecline;

        if (messageLabel != null) messageLabel.SetText(message);

        SetVisible(true);
    }

    /// <summary>
    /// Called by PlayerInteractor (or similar) when the player walks out of range
    /// while the panel is open. Closes without firing any callback.
    /// </summary>
    private void OnClosedExternally()
    {
        if (!_isOpen) return;
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void Confirm()
    {
        var cb = _onConfirm;
        Close();
        cb?.Invoke();
    }

    private void Decline()
    {
        var cb = _onDecline;
        Close();
        cb?.Invoke();
    }

    private void Close()
    {
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetVisible(bool visible)
    {
        _isOpen = visible;
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }

    private void ForceHide()
    {
        _isOpen = false;
        if (container == null) return;
        container.alpha = 0f;
        container.interactable = false;
        container.blocksRaycasts = false;
    }
}
