using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [System.Serializable]
    public struct IterationData
    {
        [Tooltip("Índice de la puerta objetivo esta iteración (0–4)")]
        [Range(0, 4)]
        public int doorIndex;

        [Tooltip("Cantidad exacta de knocks requeridos antes de abrir")]
        [Min(1)]
        public int requiredKnocks;

        [Tooltip("Spawn del jugador al inicio de la iteración y al fallar")]
        public Transform iterationSpawn;
    }

    [Header("Iteraciones (3 en total)")]
    public IterationData[] iterations = new IterationData[3];

    [Header("Fin del puzzle")]
    [Tooltip("Spawn tras completar la última iteración correctamente")]
    public Transform puzzleCompleteSpawn;

    [Header("Referencias")]
    public Transform player;
    public CamcorderTransition camcorderTransition;

    // ── Estado interno ──────────────────────────────────────────────
    private int currentIteration = 0;
    private int currentKnocks = 0;
    private bool puzzleSolved = false;

    // ── Lectura pública (útil para UI / debug) ──────────────────────
    public int CurrentIteration => currentIteration;
    public int CurrentKnocks => currentKnocks;
    public int RequiredKnocks => puzzleSolved ? 0
                                                : iterations[currentIteration].requiredKnocks;

    // ───────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ValidateSetup();
        Debug.Log("[Puzzle] Inicio — Iteración 1.");
    }

    // ── API pública llamada desde DoorInteractable ──────────────────

    /// <summary>El jugador tocó la puerta con índice doorIndex.</summary>
    public void OnDoorKnocked(int doorIndex)
    {
        if (puzzleSolved) return;
        // Knockear una puerta incorrecta no hace nada (no acumula ni castiga)
        if (doorIndex != iterations[currentIteration].doorIndex) return;

        currentKnocks++;
        Debug.Log($"[Puzzle] Knock {currentKnocks}/{iterations[currentIteration].requiredKnocks}" +
                  $" — puerta {doorIndex} (iteración {currentIteration + 1})");
    }

    public void OnDoorOpened(int doorIndex)
    {
        if (puzzleSolved) return;
        if (camcorderTransition != null && camcorderTransition.IsTransitioning) return;

        int required = iterations[currentIteration].requiredKnocks;
        int correctDoor = iterations[currentIteration].doorIndex;

        // Cualquier cosa incorrecta (puerta equivocada O knocks incorrectos) → castigo
        bool wrongDoor = doorIndex != correctDoor;
        bool wrongKnocks = currentKnocks != required;

        if (wrongDoor || wrongKnocks)
        {
            Debug.Log($"[Puzzle] ✗ Incorrecto — puerta {doorIndex} " +
                      $"(correcta: {correctDoor}), knocks {currentKnocks}/{required}. Reseteando.");
            HandleWrongOpen();
            return;
        }

        Debug.Log($"[Puzzle] ✓ Correcto — puerta {doorIndex}, {currentKnocks}/{required} knocks.");
        HandleCorrectOpen();
    }

    // ── Lógica interna ──────────────────────────────────────────────

    private void HandleCorrectOpen()
    {
        currentIteration++;
        currentKnocks = 0;

        HallwayAudioManager.Instance?.AdvanceIteration();
        GameEvents.IterationChanged(currentIteration);

        if (currentIteration >= iterations.Length)
        {
            puzzleSolved = true;
            Debug.Log("[Puzzle] Puzzle completado.");
            TeleportPlayer(puzzleCompleteSpawn, TriggerEnding);
            return;
        }

        Debug.Log($"[Puzzle] Iteración {currentIteration + 1} comienza.");
        TeleportPlayer(iterations[currentIteration].iterationSpawn);
    }

    private void HandleWrongOpen()
    {
        currentKnocks = 0;
        TeleportPlayer(iterations[currentIteration].iterationSpawn);
    }

    private void TriggerEnding()
    {
        GameEvents.PuzzleCompleted("DoorPuzzle");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void TeleportPlayer(Transform target, System.Action onComplete = null)
    {
        if (target == null) { Debug.LogError("[Puzzle] Spawn es null!"); return; }

        if (camcorderTransition != null)
            camcorderTransition.Play(onSwitch: () => MovePlayer(target), onComplete: onComplete);
        else
        {
            MovePlayer(target);
            onComplete?.Invoke();
        }
    }

    private void MovePlayer(Transform target)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.position = target.position;
        player.rotation = target.rotation;
        if (cc != null) cc.enabled = true;
    }

    // ── Validación ──────────────────────────────────────────────────

    private void ValidateSetup()
    {
        if (iterations.Length != 3)
            Debug.LogWarning("[Puzzle] 'iterations' debe tener exactamente 3 elementos.");

        for (int i = 0; i < iterations.Length; i++)
        {
            var it = iterations[i];
            if (it.requiredKnocks <= 0)
                Debug.LogWarning($"[Puzzle] Iteración {i + 1}: requiredKnocks debe ser >= 1.");
            if (it.iterationSpawn == null)
                Debug.LogWarning($"[Puzzle] Iteración {i + 1}: iterationSpawn no asignado.");
        }

        if (puzzleCompleteSpawn == null)
            Debug.LogWarning("[Puzzle] puzzleCompleteSpawn no asignado.");
        if (player == null)
            Debug.LogError("[Puzzle] player no asignado.");
        if (camcorderTransition == null)
            Debug.LogWarning("[Puzzle] CamcorderTransition no asignado — teletransporte será instantáneo.");
    }
}