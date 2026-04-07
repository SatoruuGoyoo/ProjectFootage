using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Puzzle Config")]
    [Tooltip("Índice de la puerta correcta para cada iteración (0 a 4). Tamaño = 3.")]
    public int[] correctDoorPerIteration = { 0, 2, 4 };

    [Header("Spawn Points")]
    [Tooltip("Spawn point para cada iteración. Tamaño = 3.")]
    public Transform[] spawnPoints;

    [Header("Player")]
    public Transform player;

    [Header("Transition")]
    [Tooltip("Referencia al CamcorderTransition de la escena.")]
    public CamcorderTransition camcorderTransition;

    // ── Estado interno ──────────────────────────────────────
    private int currentIteration = 0;
    private bool puzzleSolved = false;

    // ── Unity ───────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ValidateSetup();
        Debug.Log($"[Puzzle] Inicio — Iteración 1. Puerta correcta: {correctDoorPerIteration[0] + 1}");
    }

    // ── API pública ─────────────────────────────────────────

    /// <summary>
    /// Llamado por DoorTrigger cuando el player entra en una puerta.
    /// </summary>
    public void OnDoorEntered(int doorIndex)
    {
        if (puzzleSolved) return;

        // Bloqueamos nuevas entradas mientras hay transición en curso
        if (camcorderTransition != null && camcorderTransition.IsTransitioning) return;

        int humanIteration = currentIteration + 1;

        if (doorIndex == correctDoorPerIteration[currentIteration])
        {
            // ── Puerta correcta ─────────────────────────────
            Debug.Log($"[Puzzle] ✓ Puerta {doorIndex + 1} correcta en iteración {humanIteration}.");
            currentIteration++;

            if (currentIteration >= correctDoorPerIteration.Length)
            {
                puzzleSolved = true;
                Debug.Log("[Puzzle] ¡LO LOGRASTE, SALVASTE A ALYSA LU!");
                // Si querés una transición final especial podés llamarla aquí también.
                return;
            }

            Debug.Log($"[Puzzle] → Iteración {currentIteration + 1} comienza. Puerta correcta: {correctDoorPerIteration[currentIteration] + 1}");
        }
        else
        {
            // ── Puerta incorrecta ───────────────────────────
            Debug.Log($"[Puzzle] ✗ Puerta {doorIndex + 1} incorrecta. Volviendo al spawn de iteración {humanIteration}.");
        }

        // Siempre volvemos al spawn de la iteración actual (correcta o no)
        TeleportToSpawn();
    }

    // ── Privado ─────────────────────────────────────────────

    private void TeleportToSpawn()
    {
        if (player == null) { Debug.LogWarning("[Puzzle] Player no asignado."); return; }
        if (spawnPoints == null || currentIteration >= spawnPoints.Length)
        {
            Debug.LogWarning("[Puzzle] SpawnPoint faltante para iteración actual.");
            return;
        }

        if (camcorderTransition != null)
        {
            // El teletransporte ocurre en el pico de la estática (onSwitch),
            // invisible para el jugador.
            camcorderTransition.Play(
                onSwitch: () => MovePlayerToSpawn(),
                onComplete: null   // podés pasar un callback si necesitás algo al terminar
            );
        }
        else
        {
            // Fallback sin transición (por si no está asignado en Inspector)
            Debug.LogWarning("[Puzzle] CamcorderTransition no asignado — teletransporte directo.");
            MovePlayerToSpawn();
        }
    }

    /// <summary>
    /// Mueve físicamente al player al spawn. Se llama desde el onSwitch de la transición,
    /// cuando la pantalla está completamente cubierta de estática.
    /// </summary>
    private void MovePlayerToSpawn()
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = spawnPoints[currentIteration].position;
        player.rotation = spawnPoints[currentIteration].rotation;

        if (cc != null) cc.enabled = true;
    }

    private void ValidateSetup()
    {
        if (correctDoorPerIteration.Length != 3)
            Debug.LogWarning("[Puzzle] correctDoorPerIteration debería tener exactamente 3 elementos.");

        if (spawnPoints == null || spawnPoints.Length < 3)
            Debug.LogWarning("[Puzzle] spawnPoints debería tener al menos 3 transforms.");

        if (camcorderTransition == null)
            Debug.LogWarning("[Puzzle] CamcorderTransition no asignado. El teletransporte será instantáneo.");
    }
}