using UnityEngine;
using FMODUnity;

public class WindowInteractable : Interactable
{
    [System.Serializable]
    public class IterationEntry
    {
        [Tooltip("Número de iteración del CorridorTeleporter en que aplica esta entrada (empieza en 1).")]
        public int iteration = 1;

        [Tooltip("GameObject que se activa mientras el jugador mira en esta iteración. " +
                 "Se desactiva al dejar de mirar o al cambiar de iteración.")]
        public GameObject visibleObject;

        [Tooltip("Sonido FMOD que suena al abrir la ventana en esta iteración. Opcional.")]
        public EventReference sound;
    }

    [Header("Interacción")]
    [SerializeField] private string promptOpen = "Mirar por la ventana";
    [SerializeField] private string promptClose = "Dejar de mirar";
    [SerializeField] private string feedbackOpen = "";
    [SerializeField] private string feedbackClose = "";

    [Header("Confirmation")]
    [SerializeField] private bool requiresConfirmation = false;
    [TextArea(2, 5)]
    [SerializeField] private string confirmationText = "";

    [Header("Cámara")]
    [SerializeField] private Camera windowCamera;
    [SerializeField] private bool toggleCameraGameObject = true;

    [Header("Corridor Teleporters")]
    [SerializeField] private GameObject[] teleporterObjects;

    [Header("Contenido por iteración")]
    [SerializeField] private IterationEntry[] iterationEntries;

    private bool _isLooking;
    private bool _pendingConfirmation;
    private IterationEntry _activeEntry;

    public override string PromptMessage => _isLooking ? promptClose : promptOpen;
    public override bool CanInteract => !_isLooking && !_pendingConfirmation;
    public override bool IsActive => _isLooking;
    public override bool BlockMovement => true;

    private void Awake()
    {
        SetCameraActive(false);
        SetTeleporterObjectsActive(false);

        if (iterationEntries == null) return;
        foreach (var entry in iterationEntries)
            if (entry.visibleObject != null)
                entry.visibleObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnConfirmationClosed += OnConfirmationClosed;
        CorridorTeleporter.OnIterationChanged += OnIterationChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        CorridorTeleporter.OnIterationChanged -= OnIterationChanged;
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

    private void OnIterationChanged(int iteration)
    {
        SetTeleporterObjectsActive(false);
        HideActiveEntry();
    }

    private void StartLooking()
    {
        _isLooking = true;
        SetCameraActive(true);
        RefreshIterationContent(CorridorTeleporter.IterationCount);
        EnterInteractionMode();

        if (!string.IsNullOrEmpty(feedbackOpen))
            GameEvents.FeedbackMessage(feedbackOpen, uiPosition);
    }

    private void StopLooking()
    {
        _isLooking = false;
        HideActiveEntry();
        SetCameraActive(false);
        SetTeleporterObjectsActive(true);
        ExitInteractionMode();

        if (!string.IsNullOrEmpty(feedbackClose))
            GameEvents.FeedbackMessage(feedbackClose, uiPosition);
    }

    private void RefreshIterationContent(int iteration)
    {
        HideActiveEntry();

        _activeEntry = FindEntry(iteration);
        if (_activeEntry == null) return;

        if (_activeEntry.visibleObject != null)
            _activeEntry.visibleObject.SetActive(true);

        if (!_activeEntry.sound.IsNull)
            RuntimeManager.PlayOneShot(_activeEntry.sound, transform.position);
    }

    private void HideActiveEntry()
    {
        if (_activeEntry == null) return;
        if (_activeEntry.visibleObject != null)
            _activeEntry.visibleObject.SetActive(false);
        _activeEntry = null;
    }

    private IterationEntry FindEntry(int iteration)
    {
        if (iterationEntries == null) return null;
        foreach (var entry in iterationEntries)
            if (entry.iteration == iteration) return entry;
        return null;
    }

    private void SetCameraActive(bool active)
    {
        if (windowCamera == null) return;

        if (toggleCameraGameObject)
            windowCamera.gameObject.SetActive(active);
        else
            windowCamera.enabled = active;
    }

    private void SetTeleporterObjectsActive(bool active)
    {
        if (teleporterObjects == null) return;
        foreach (var go in teleporterObjects)
            if (go != null)
                go.SetActive(active);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (windowCamera == null)
            Debug.LogWarning("[WindowInteractable] No hay cámara asignada.", this);
    }
#endif
}