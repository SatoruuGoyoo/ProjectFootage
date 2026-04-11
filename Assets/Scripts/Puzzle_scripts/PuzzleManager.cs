using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [System.Serializable]
    public struct IterationData
    {
        [Tooltip("3 door indices in correct order. 0=1A 1=1B 2=1C 3=1D 4=1E 5=1F")]
        public int[] correctSequence;
        [Tooltip("Player spawn point at the start of this iteration")]
        public Transform iterationSpawn;
    }

    [Header("Iterations")]
    public IterationData[] iterations = new IterationData[3];

    [Header("Door Spawns")]
    [Tooltip("One Transform per door (0-5). Player spawns here after a correct door.")]
    public Transform[] doorCorrectSpawns = new Transform[6];

    [Header("References")]
    public Transform player;
    public CamcorderTransition camcorderTransition;

    private int currentIteration = 0;
    private int currentStep = 0;
    private bool puzzleSolved = false;

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

    public void OnDoorEntered(int doorIndex)
    {
        if (puzzleSolved) return;
        if (camcorderTransition != null && camcorderTransition.IsTransitioning) return;

        int expected = iterations[currentIteration].correctSequence[currentStep];

        if (doorIndex == expected)
            HandleCorrectDoor(doorIndex);
        else
            HandleWrongDoor();
    }

    private void HandleCorrectDoor(int doorIndex)
    {
        Debug.Log($"[Puzzle] Correct — {DoorName(doorIndex)} | iter {currentIteration + 1}, step {currentStep + 1}/3");
        currentStep++;

        if (currentStep >= 3)
            AdvanceIteration();
        else
            TeleportPlayer(doorCorrectSpawns[doorIndex]);
    }

    private void HandleWrongDoor()
    {
        Debug.Log($"[Puzzle] Wrong door — resetting iteration {currentIteration + 1}");
        currentStep = 0;
        TeleportPlayer(iterations[currentIteration].iterationSpawn);
    }

    private void AdvanceIteration()
    {
        currentIteration++;
        currentStep = 0;
        HallwayAudioManager.Instance?.AdvanceIteration();
        GameEvents.IterationChanged(currentIteration);

        if (currentIteration >= iterations.Length)
        {
            puzzleSolved = true;
            Debug.Log("[Puzzle] Puzzle complete.");
            TriggerEnding();
            return;
        }

        Debug.Log($"[Puzzle] Iteration {currentIteration + 1} begins.");
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

    private void TeleportPlayer(Transform target)
    {
        if (target == null) { Debug.LogError("[Puzzle] Spawn is null!"); return; }

        if (camcorderTransition != null)
            camcorderTransition.Play(onSwitch: () => MovePlayer(target), onComplete: null);
        else
            MovePlayer(target);
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
            Debug.LogWarning("[Puzzle] iterations should have exactly 3 elements.");
        foreach (var iter in iterations)
            if (iter.correctSequence == null || iter.correctSequence.Length != 3)
                Debug.LogWarning("[Puzzle] Each iteration needs exactly 3 doors in correctSequence.");
        if (doorCorrectSpawns.Length < 6)
            Debug.LogWarning("[Puzzle] doorCorrectSpawns needs 6 transforms (one per door).");
        if (camcorderTransition == null)
            Debug.LogWarning("[Puzzle] CamcorderTransition not assigned — teleport will be instant.");
    }

    private static string DoorName(int i)
    {
        string[] names = { "1A", "1B", "1C", "1D", "1E", "1F" };
        return i >= 0 && i < names.Length ? names[i] : $"?{i}";
    }
}