using UnityEngine;
using UnityEngine.Events;

public class WindowInteractable : Interactable
{
    [Header("Prompts")]
    [SerializeField] private string promptOpen = "Mirar por la ventana";
    [SerializeField] private string promptClose = "Dejar de mirar";

    [Header("Feedback")]
    [SerializeField] private string feedbackOpen = "";
    [SerializeField] private string feedbackClose = "";
    [SerializeField] private float feedbackDuration = -1f;

    [Header("Confirmation")]
    [SerializeField] private bool requiresConfirmation = false;
    [TextArea(2, 5)]
    [SerializeField] private string confirmationText = "";

    [Header("Cámara")]
    [SerializeField] private Camera windowCamera;
    [SerializeField] private bool toggleCameraGameObject = true;

    [Header("Events")]
    [Tooltip("Sólo la primera vez que el jugador mira.")]
    public UnityEvent OnFirstLook;
    [Tooltip("Cada vez que el jugador mira.")]
    public UnityEvent OnLook;
    [Tooltip("Sólo la primera vez que el jugador deja de mirar.")]
    public UnityEvent OnFirstStopLooking;
    [Tooltip("Cada vez que el jugador deja de mirar.")]
    public UnityEvent OnStopLooking;

    private bool _isLooking;
    private bool _pendingConfirmation;
    private bool _lookedOnce;
    private bool _stoppedOnce;

    public override string PromptMessage => _isLooking ? promptClose : promptOpen;
    public override bool CanInteract => !_isLooking && !_pendingConfirmation;
    public override bool IsActive => _isLooking;
    public override bool BlockMovement => true;

    private void Awake() => SetCameraActive(false);

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        _pendingConfirmation = false;
        if (_isLooking) StopLooking();
    }

    public override void Interact()
    {
        if (!CanInteract) return;

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined, uiPosition);
            return;
        }

        StartLooking();
    }

    public override void Cancel()
    {
        if (!_isLooking) return;
        StopLooking();
    }

    private void OnConfirmed()
    {
        _pendingConfirmation = false;
        StartLooking();
    }

    private void OnDeclined() => _pendingConfirmation = false;

    private void OnConfirmationClosed() => _pendingConfirmation = false;

    private void StartLooking()
    {
        _isLooking = true;
        SetCameraActive(true);
        EnterInteractionMode();

        if (!string.IsNullOrEmpty(feedbackOpen))
            GameEvents.FeedbackMessage(feedbackOpen, uiPosition, feedbackDuration);

        if (!_lookedOnce)
        {
            _lookedOnce = true;
            OnFirstLook?.Invoke();
        }

        OnLook?.Invoke();
    }

    private void StopLooking()
    {
        _isLooking = false;
        SetCameraActive(false);
        ExitInteractionMode();

        if (!string.IsNullOrEmpty(feedbackClose))
            GameEvents.FeedbackMessage(feedbackClose, uiPosition, feedbackDuration);

        if (!_stoppedOnce)
        {
            _stoppedOnce = true;
            OnFirstStopLooking?.Invoke();
        }

        OnStopLooking?.Invoke();
    }

    private void SetCameraActive(bool active)
    {
        if (windowCamera == null) return;

        if (toggleCameraGameObject)
            windowCamera.gameObject.SetActive(active);
        else
            windowCamera.enabled = active;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (windowCamera == null)
            Debug.LogWarning("[WindowInteractable] No hay cámara asignada.", this);
    }
#endif
}