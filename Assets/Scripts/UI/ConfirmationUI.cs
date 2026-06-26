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

    private bool _isOpen;
    private System.Action _onConfirm;
    private System.Action _onDecline;
    private PlayerInput _input;

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

    private void OnRequested(string message, System.Action onConfirm, System.Action onDecline)
    {
        // Confirmation tiene prioridad máxima: cierra todo lo que esté debajo.
        UILayerManager.TryShow(UILayerManager.Layer.Confirmation, ForceHide);

        _onConfirm = onConfirm;
        _onDecline = onDecline;
        if (messageLabel != null) messageLabel.SetText(message);
        SetVisible(true);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
    }

    private void OnClosedExternally()
    {
        if (!_isOpen) return;
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
        UILayerManager.Release(UILayerManager.Layer.Confirmation);
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

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
        UILayerManager.Release(UILayerManager.Layer.Confirmation);
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

    // Callback registrado en UILayerManager (en la práctica nunca se llama
    // porque Confirmation es la prioridad más alta, pero lo dejamos por consistencia).
    private void ForceHide()
    {
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
        UILayerManager.Release(UILayerManager.Layer.Confirmation);
    }

    private void SetVisible(bool visible)
    {
        _isOpen = visible;
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }
}