using UnityEngine;
using UnityEngine.Events;

public class WindowInteractable : Interactable
{
    [Header("Prompts")]
    [SerializeField] private string promptOpen = "Mirar por la ventana";
    [SerializeField] private string promptClose = "Dejar de mirar";

    [Header("Subtítulos")]
    [SerializeField] private SubtitleBlock subtitles;
    [SerializeField] private SubtitleEntry[] linesOnLook;
    [SerializeField] private SubtitleEntry[] linesOnStopLooking;

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
    public override bool CanInteract => !_pendingConfirmation;
    public override bool IsActive => _isLooking;
    public override bool BlockMovement => true;

    private void Awake() => SetCameraActive(false);

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        _pendingConfirmation = false;
        if (_isLooking) StopLooking();
        if (subtitles != null) subtitles.Hide();
    }

    public override void Interact()
    {
        if (!CanInteract) return;

        if (_isLooking)
        {
            StopLooking();
            return;
        }

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            RequestConfirmation(confirmationText, OnConfirmed, OnDeclined);
            return;
        }

        StartLooking();
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

        PlayLines(linesOnLook);

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

        PlayLines(linesOnStopLooking);

        if (!_stoppedOnce)
        {
            _stoppedOnce = true;
            OnFirstStopLooking?.Invoke();
        }

        OnStopLooking?.Invoke();
    }

    private void PlayLines(SubtitleEntry[] entries)
    {
        if (subtitles == null) return;
        if (entries == null || entries.Length == 0) return;
        subtitles.ShowSequence(entries);
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