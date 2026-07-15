//using UnityEngine;
//using FMODUnity;

//public class WindowInteractable : MonoBehaviour, IInteractable
//{
//    [System.Serializable]
//    public class IterationEntry
//    {
//        [Tooltip("Número de iteración del CorridorTeleporter en que aplica esta entrada (empieza en 1).")]
//        public int iteration = 1;

//        [Tooltip("GameObject que se activa mientras el jugador mira en esta iteración. " +
//                 "Se desactiva al dejar de mirar o al cambiar de iteración.")]
//        public GameObject visibleObject;

//        [Tooltip("Sonido FMOD que suena al abrir la ventana en esta iteración. Opcional.")]
//        public EventReference sound;
//    }

//    [Header("Interacción")]
//    [SerializeField] private string promptOpen = "Mirar por la ventana";
//    [SerializeField] private string promptClose = "Dejar de mirar";
//    [SerializeField] private string feedbackOpen = "";
//    [SerializeField] private string feedbackClose = "";

//    [Header("Confirmation")]
//    [SerializeField] private bool requiresConfirmation = false;
//    [SerializeField] private string confirmationText = "";

//    [Header("Cámara")]
//    [SerializeField] private Camera windowCamera;
//    [SerializeField] private bool toggleCameraGameObject = true;

//    [Header("Corridor Teleporters")]
//    [SerializeField] private GameObject[] teleporterObjects;

//    [Header("Contenido por iteración")]
//    [SerializeField] private IterationEntry[] iterationEntries;

//    private bool _isLooking;
//    private bool _pendingConfirmation;
//    private IterationEntry _activeEntry;

//    public string PromptMessage => _isLooking
//        ? (string.IsNullOrEmpty(promptClose) ? "Dejar de mirar" : promptClose)
//        : (string.IsNullOrEmpty(promptOpen) ? "Mirar" : promptOpen);
//    public bool CanInteract => true;
//    public bool BlockMovement => true;

//    public void Interact()
//    {
//        if (_pendingConfirmation) return;

//        if (_isLooking)
//        {
//            StopLooking();
//            return;
//        }

//        if (requiresConfirmation)
//        {
//            _pendingConfirmation = true;
//            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined);
//        }
//        else
//        {
//            StartLooking();
//        }
//    }

//    private void OnEnable()
//    {
//        GameEvents.OnConfirmationClosed += OnConfirmationClosed;
//        CorridorTeleporter.OnIterationChanged += OnIterationChanged;
//    }

//    private void OnDisable()
//    {
//        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
//        CorridorTeleporter.OnIterationChanged -= OnIterationChanged;
//        _pendingConfirmation = false;
//    }

//    private void OnConfirmed()
//    {
//        _pendingConfirmation = false;
//        StartLooking();
//    }

//    private void OnDeclined()
//    {
//        _pendingConfirmation = false;
//    }

//    private void OnConfirmationClosed()
//    {
//        _pendingConfirmation = false;
//    }

//    private void OnIterationChanged(int iteration)
//    {
//        SetTeleporterObjectsActive(false);
//        HideActiveEntry();
//    }

//    private void RefreshIterationContent(int iteration)
//    {
//        HideActiveEntry();

//        _activeEntry = FindEntry(iteration);
//        if (_activeEntry == null) return;

//        if (_activeEntry.visibleObject != null)
//            _activeEntry.visibleObject.SetActive(true);

//        if (!_activeEntry.sound.IsNull)
//            RuntimeManager.PlayOneShot(_activeEntry.sound, transform.position);
//    }

//    private void HideActiveEntry()
//    {
//        if (_activeEntry == null) return;
//        if (_activeEntry.visibleObject != null)
//            _activeEntry.visibleObject.SetActive(false);
//        _activeEntry = null;
//    }

//    private IterationEntry FindEntry(int iteration)
//    {
//        if (iterationEntries == null) return null;
//        foreach (var e in iterationEntries)
//            if (e.iteration == iteration) return e;
//        return null;
//    }

//    private void StartLooking()
//    {
//        _isLooking = true;
//        SetCameraActive(true);
//        RefreshIterationContent(CorridorTeleporter.IterationCount);
//        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);

//        if (!string.IsNullOrEmpty(feedbackOpen))
//            GameEvents.FeedbackMessage(feedbackOpen);
//    }

//    private void StopLooking()
//    {
//        _isLooking = false;
//        HideActiveEntry();
//        SetCameraActive(false);
//        SetTeleporterObjectsActive(true);
//        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);

//        if (!string.IsNullOrEmpty(feedbackClose))
//            GameEvents.FeedbackMessage(feedbackClose);
//    }

//    private void SetCameraActive(bool active)
//    {
//        if (windowCamera == null) return;

//        if (toggleCameraGameObject)
//            windowCamera.gameObject.SetActive(active);
//        else
//            windowCamera.enabled = active;
//    }

//    private void SetTeleporterObjectsActive(bool active)
//    {
//        if (teleporterObjects == null) return;
//        foreach (var go in teleporterObjects)
//            if (go != null)
//                go.SetActive(active);
//    }

//    private void Awake()
//    {
//        SetCameraActive(false);
//        SetTeleporterObjectsActive(false);

//        if (iterationEntries != null)
//            foreach (var e in iterationEntries)
//                if (e.visibleObject != null)
//                    e.visibleObject.SetActive(false);
//    }

//#if UNITY_EDITOR
//    private void OnValidate()
//    {
//        if (windowCamera == null)
//            Debug.LogWarning("[WindowInteractable] No hay cámara asignada.", this);
//    }
//#endif
//}