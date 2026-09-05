using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FMODUnity;

public class ConfirmationUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private UIPositioner positioner;

    [Header("Labels")]
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private TMP_Text yesLabel;
    [SerializeField] private TMP_Text noLabel;

    [Header("Default button labels")]
    [SerializeField] private string yesText = "Sí";
    [SerializeField] private string noText = "No";

    [Header("Selection Highlight")]
    [SerializeField] private Image yesHighlight;
    [SerializeField] private Image noHighlight;
    [SerializeField] private Color highlightColor = new Color(0.1f, 0.2f, 0.8f, 1f);

    [Header("Audio")]
    [SerializeField] private EventReference selectSound;
    [SerializeField] private EventReference confirmSound;

    private bool _isOpen;
    private bool _yesSelected;
    private bool _navigateNeutral = true;
    private bool _suppressSubmitThisFrame;
    private System.Action _onConfirm;
    private System.Action _onDecline;
    private InputAction _navigateAction;
    private InputAction _submitAction;

    private void Awake()
    {
        if (yesLabel != null) yesLabel.SetText(yesText);
        if (noLabel != null) noLabel.SetText(noText);
        if (yesHighlight != null) yesHighlight.color = highlightColor;
        if (noHighlight != null) noHighlight.color = highlightColor;
        ForceHide();
    }

    private void Start()
    {
        _navigateAction = PlayerInput.Actions.UI.Navigate;
        _submitAction = PlayerInput.Actions.UI.Submit;
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
        if (!_isOpen) return;

        float h = _navigateAction.ReadValue<Vector2>().x;

        if (_navigateNeutral)
        {
            if (h > 0.5f) SetSelected(false, playSound: true);
            else if (h < -0.5f) SetSelected(true, playSound: true);

            if (Mathf.Abs(h) > 0.5f) _navigateNeutral = false;
        }
        else if (Mathf.Abs(h) < 0.1f)
        {
            _navigateNeutral = true;
        }

        if (_suppressSubmitThisFrame)
        {
            _suppressSubmitThisFrame = false;
            return;
        }

        if (_submitAction.WasPressedThisFrame())
        {
            if (!confirmSound.IsNull) RuntimeManager.PlayOneShot(confirmSound, transform.position);

            if (_yesSelected) Confirm();
            else Decline();
        }
    }

    private void SetSelected(bool yes, bool playSound = false)
    {
        if (playSound && yes != _yesSelected && !selectSound.IsNull)
            RuntimeManager.PlayOneShot(selectSound, transform.position);

        _yesSelected = yes;
        if (yesHighlight != null) yesHighlight.gameObject.SetActive(yes);
        if (noHighlight != null) noHighlight.gameObject.SetActive(!yes);
    }

    private void OnRequested(string message, System.Action onConfirm, System.Action onDecline, UIPositioner.ScreenPosition position)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.Confirmation, this, position, ForceHide)) return;
        positioner?.SetPosition(position);
        _onConfirm = onConfirm;
        _onDecline = onDecline;
        if (messageLabel != null) messageLabel.SetText(message);
        SetSelected(false);
        _navigateNeutral = Mathf.Abs(_navigateAction.ReadValue<Vector2>().x) < 0.5f;
        _suppressSubmitThisFrame = true;
        SetVisible(true);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
    }

    private void OnClosedExternally()
    {
        if (!_isOpen) return;
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
        UILayerManager.Release(UILayerManager.Layer.Confirmation, this);
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
        UILayerManager.Release(UILayerManager.Layer.Confirmation, this);
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        GameEvents.CloseConfirmation();
    }

    private void ForceHide()
    {
        _onConfirm = null;
        _onDecline = null;
        SetVisible(false);
        UILayerManager.Release(UILayerManager.Layer.Confirmation, this);
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