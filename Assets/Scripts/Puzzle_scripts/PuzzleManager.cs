using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    // ─── Datos de iteración ───────────────────────────────────────────────────

    [System.Serializable]
    public struct IterationData
    {
        [Tooltip("3 índices en orden correcto. 0=1A 1=1B 2=1C 3=1D 4=1E 5=1F")]
        public int[] correctSequence;
        [Tooltip("Spawn inicial de esta iteración")]
        public Transform iterationSpawn;
    }

    [Header("Iteraciones")]
    public IterationData[] iterations = new IterationData[3];

    [Header("Spawns de puerta correcta")]
    [Tooltip("Un Transform por puerta (0-5). Al acertar, el jugador aparece acá.")]
    public Transform[] doorCorrectSpawns = new Transform[6];

    [Header("Referencias")]
    public Transform player;
    public CamcorderTransition camcorderTransition;

    // ─── Estado interno ───────────────────────────────────────────────────────

    private int _currentIteration = 0;
    private int _currentStep = 0;
    private bool _puzzleSolved = false;

    // ─── Unity ───────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ValidateSetup();
        Debug.Log($"[Puzzle] Inicio — Iteración 1.");
    }

    // ─── API pública ──────────────────────────────────────────────────────────

    public void OnDoorEntered(int doorIndex)
    {
        if (_puzzleSolved) return;
        if (camcorderTransition != null && camcorderTransition.IsTransitioning) return;

        int expected = iterations[_currentIteration].correctSequence[_currentStep];

        if (doorIndex == expected)
            HandleCorrectDoor(doorIndex);
        else
            HandleWrongDoor();
    }

    // ─── Lógica ───────────────────────────────────────────────────────────────

    void HandleCorrectDoor(int doorIndex)
    {
        Debug.Log($"[Puzzle] ✓ {DoorName(doorIndex)} — iter {_currentIteration + 1}, paso {_currentStep + 1}/3");
        _currentStep++;

        if (_currentStep >= 3)
        {
            AdvanceIteration();
        }
        else
        {
            // Spawn al otro lado de la puerta correcta (sin volver al inicio)
            TeleportPlayer(doorCorrectSpawns[doorIndex]);
        }
    }

    void HandleWrongDoor()
    {
        Debug.Log($"[Puzzle] ✗ Puerta incorrecta — reiniciando iteración {_currentIteration + 1}");
        _currentStep = 0;
        TeleportPlayer(iterations[_currentIteration].iterationSpawn);
    }

    void AdvanceIteration()
    {
        _currentIteration++;
        _currentStep = 0;
        HallwayAudioManager.Instance?.AdvanceIteration();
        GameEvents.IterationChanged(_currentIteration);

        if (_currentIteration >= iterations.Length)
        {
            _puzzleSolved = true;
            Debug.Log("[Puzzle] ★ Puzzle completo.");
            TriggerEnding();
            return;
        }

        Debug.Log($"[Puzzle] → Iteración {_currentIteration + 1} comienza.");
        TeleportPlayer(iterations[_currentIteration].iterationSpawn);
    }

    void TriggerEnding()
    {
        GameEvents.PuzzleCompleted("DoorPuzzle");

        // Acá enchufás el jumpscare antes del Quit
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ─── Teleport ─────────────────────────────────────────────────────────────

    void TeleportPlayer(Transform target)
    {
        if (target == null) { Debug.LogError("[Puzzle] Spawn null!"); return; }

        if (camcorderTransition != null)
        {
            camcorderTransition.Play(
                onSwitch: () => MovePlayer(target),
                onComplete: null
            );
        }
        else
        {
            Debug.LogWarning("[Puzzle] Sin CamcorderTransition — teleport directo.");
            MovePlayer(target);
        }
    }

    void MovePlayer(Transform target)
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = target.position;
        player.rotation = target.rotation;

        if (cc != null) cc.enabled = true;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    void ValidateSetup()
    {
        if (iterations.Length != 3)
            Debug.LogWarning("[Puzzle] iterations debería tener 3 elementos.");
        foreach (var iter in iterations)
            if (iter.correctSequence == null || iter.correctSequence.Length != 3)
                Debug.LogWarning("[Puzzle] Cada iteración necesita exactamente 3 puertas en correctSequence.");
        if (doorCorrectSpawns.Length < 6)
            Debug.LogWarning("[Puzzle] doorCorrectSpawns necesita 6 transforms (uno por puerta).");
        if (camcorderTransition == null)
            Debug.LogWarning("[Puzzle] CamcorderTransition no asignado — teleport será instantáneo.");
    }

    static string DoorName(int i)
    {
        string[] names = { "1A", "1B", "1C", "1D", "1E", "1F" };
        return i >= 0 && i < names.Length ? names[i] : $"?{i}";
    }
}