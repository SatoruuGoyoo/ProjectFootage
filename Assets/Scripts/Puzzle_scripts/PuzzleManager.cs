using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [System.Serializable]
    public struct IterationData
    {
        [Tooltip("Target door index for this iteration (0-4)")]
        [Range(0, 4)]
        public int doorIndex;

        [Tooltip("Exact number of knocks required before opening")]
        [Min(1)]
        public int requiredKnocks;

        [Tooltip("Player spawn point at iteration start and on failure")]
        public Transform iterationSpawn;
    }

    [Header("Iterations")]
    public IterationData[] iterations = new IterationData[3];

    [Header("Puzzle End")]
    [Tooltip("Spawn point after completing the last iteration correctly")]
    public Transform puzzleCompleteSpawn;

    [Header("References")]
    public Transform player;
    public CamcorderTransition camcorderTransition;

    private int currentIteration = 0;
    private int currentKnocks = 0;
    private bool puzzleSolved = false;

    public int CurrentIteration => currentIteration;
    public int CurrentKnocks => currentKnocks;
    public int RequiredKnocks => puzzleSolved ? 0 : iterations[currentIteration].requiredKnocks;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ValidateSetup();
        Debug.Log("[Puzzle] Start — Iteration 1.");
    }

    public void OnDoorKnocked(int doorIndex)
    {
        if (puzzleSolved) return;
        if (doorIndex != iterations[currentIteration].doorIndex) return;

        currentKnocks++;
        Debug.Log($"[Puzzle] Knock {currentKnocks}/{iterations[currentIteration].requiredKnocks} — door {doorIndex} (iteration {currentIteration + 1})");
    }

    public void OnDoorOpened(int doorIndex)
    {
        if (puzzleSolved) return;
        if (camcorderTransition != null && camcorderTransition.IsTransitioning) return;

        int required = iterations[currentIteration].requiredKnocks;
        int correctDoor = iterations[currentIteration].doorIndex;

        if (doorIndex != correctDoor || currentKnocks != required)
        {
            Debug.Log($"[Puzzle] Wrong — door {doorIndex} (correct: {correctDoor}), knocks {currentKnocks}/{required}. Resetting.");
            HandleWrongOpen();
            return;
        }

        Debug.Log($"[Puzzle] Correct — door {doorIndex}, {currentKnocks}/{required} knocks.");
        HandleCorrectOpen();
    }

    private void HandleCorrectOpen()
    {
        currentIteration++;
        currentKnocks = 0;
        HallwayAudioManager.Instance?.AdvanceIteration();
        GameEvents.IterationChanged(currentIteration);

        if (currentIteration >= iterations.Length)
        {
            puzzleSolved = true;
            Debug.Log("[Puzzle] Puzzle complete.");
            TeleportPlayer(puzzleCompleteSpawn, TriggerEnding);
            return;
        }

        Debug.Log($"[Puzzle] Iteration {currentIteration + 1} begins.");
        TeleportPlayer(iterations[currentIteration].iterationSpawn);
    }

    private void HandleWrongOpen()
    {
        currentKnocks = 0;
        GameEvents.IterationChanged(currentIteration);
        TeleportPlayer(iterations[currentIteration].iterationSpawn);
    }

    private void TriggerEnding()
    {
        GameEvents.PuzzleCompleted("DoorPuzzle");
        UnityEngine.SceneManagement.SceneManager.LoadScene("SCN_MainMenu");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void TeleportPlayer(Transform target, System.Action onComplete = null)
    {
        if (target == null) { Debug.LogError("[Puzzle] Spawn is null!"); return; }

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

    private void ValidateSetup()
    {
        if (iterations.Length != 3)
            Debug.LogWarning("[Puzzle] 'iterations' should have exactly 3 elements.");

        for (int i = 0; i < iterations.Length; i++)
        {
            if (iterations[i].requiredKnocks <= 0)
                Debug.LogWarning($"[Puzzle] Iteration {i + 1}: requiredKnocks must be >= 1.");
            if (iterations[i].iterationSpawn == null)
                Debug.LogWarning($"[Puzzle] Iteration {i + 1}: iterationSpawn not assigned.");
        }

        if (puzzleCompleteSpawn == null)
            Debug.LogWarning("[Puzzle] puzzleCompleteSpawn not assigned.");
        if (player == null)
            Debug.LogError("[Puzzle] player not assigned.");
        if (camcorderTransition == null)
            Debug.LogWarning("[Puzzle] CamcorderTransition not assigned — teleport will be instant.");
    }
}