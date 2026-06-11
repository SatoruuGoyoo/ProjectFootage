using UnityEngine;
using FMODUnity;

/// <summary>
/// Interactable ventana: al interactuar activa una cámara (modo "mirar por la ventana").
/// Al volver a presionar Interact, desactiva la cámara.
/// 
/// Loop por iteración:
///   1. Mirar por la ventana (abrir + cerrar) → activa los GOs de los teleporters
///   2. Jugador pasa por el teleporter → iteración avanza → desactiva los GOs
///   3. Volver a mirar → reactiva los GOs → siguiente iteración
///
/// Soporta confirmación opcional e contenido visual/sonoro por iteración.
/// </summary>
public class WindowInteractable : MonoBehaviour, IInteractable
{
    // ── Entrada por iteración ─────────────────────────────────────────

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

    // ── Inspector ─────────────────────────────────────────────────────

    [Header("Interacción")]
    [SerializeField] private string promptOpen = "Mirar por la ventana";
    [SerializeField] private string promptClose = "Dejar de mirar";
    [SerializeField] private string feedbackOpen = "";
    [SerializeField] private string feedbackClose = "";

    [Header("Confirmation")]
    [SerializeField] private bool requiresConfirmation = false;
    [SerializeField] private string confirmationText = "";

    [Header("Cámara")]
    [Tooltip("La cámara que se activa al mirar por la ventana.")]
    [SerializeField] private Camera windowCamera;
    [Tooltip("Si es true, desactiva el GameObject de la cámara. " +
             "Si es false, solo desactiva el componente Camera.")]
    [SerializeField] private bool toggleCameraGameObject = true;

    [Header("Corridor Teleporters")]
    [Tooltip("GOs de los teleporters. Se activan tras cerrar la ventana y se desactivan al usarlos.")]
    [SerializeField] private GameObject[] teleporterObjects;

    [Header("Contenido por iteración")]
    [Tooltip("Cada entrada define qué se ve y qué suena al mirar en una iteración específica.")]
    [SerializeField] private IterationEntry[] iterationEntries;

    // ── Estado ────────────────────────────────────────────────────────

    private bool _isLooking;
    private bool _pendingConfirmation;
    private IterationEntry _activeEntry;

    // ── IInteractable ─────────────────────────────────────────────────

    public string PromptMessage => _isLooking
        ? (string.IsNullOrEmpty(promptClose) ? "Dejar de mirar" : promptClose)
        : (string.IsNullOrEmpty(promptOpen) ? "Mirar" : promptOpen);
    public bool CanInteract => true;

    public void Interact()
    {
        if (_pendingConfirmation) return;

        if (_isLooking)
        {
            StopLooking();
            return;
        }

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined);
        }
        else
        {
            StartLooking();
        }
    }

    // ── OnEnable / OnDisable ──────────────────────────────────────────

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
    }

    // ── Confirmation callbacks ────────────────────────────────────────

    private void OnConfirmed()
    {
        _pendingConfirmation = false;
        StartLooking();
    }

    private void OnDeclined()
    {
        _pendingConfirmation = false;
    }

    private void OnConfirmationClosed()
    {
        _pendingConfirmation = false;
    }

    // ── Iteración ─────────────────────────────────────────────────────

    private void OnIterationChanged(int iteration)
    {
        // El jugador pasó por el teleporter: desactivar GOs y contenido visible
        SetTeleporterObjectsActive(false);
        HideActiveEntry();
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
        foreach (var e in iterationEntries)
            if (e.iteration == iteration) return e;
        return null;
    }

    // ── Lógica interna ────────────────────────────────────────────────

    private void StartLooking()
    {
        _isLooking = true;
        SetCameraActive(true);
        RefreshIterationContent(CorridorTeleporter.IterationCount);

        if (!string.IsNullOrEmpty(feedbackOpen))
            GameEvents.FeedbackMessage(feedbackOpen);
    }

    private void StopLooking()
    {
        _isLooking = false;
        HideActiveEntry();
        SetCameraActive(false);

        // Cerrar la ventana activa los teleporters para que el jugador pueda pasar
        SetTeleporterObjectsActive(true);

        if (!string.IsNullOrEmpty(feedbackClose))
            GameEvents.FeedbackMessage(feedbackClose);
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

    // ── Init ──────────────────────────────────────────────────────────

    private void Awake()
    {
        SetCameraActive(false);

        // Teleporters desactivados hasta que el jugador mire por la ventana
        SetTeleporterObjectsActive(false);

        // Contenido de iteraciones desactivado
        if (iterationEntries != null)
            foreach (var e in iterationEntries)
                if (e.visibleObject != null)
                    e.visibleObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (windowCamera == null)
            Debug.LogWarning("[WindowInteractable] No hay cámara asignada.", this);
    }
#endif
}