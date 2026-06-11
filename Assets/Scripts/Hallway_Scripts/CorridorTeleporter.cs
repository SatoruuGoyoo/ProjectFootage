using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class CorridorTeleporter : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private CorridorTeleporter pairedTeleporter;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Range(0.05f, 2f)] private float cooldownSeconds = 0.3f;

    [Header("Modo")]
    [Tooltip("Si está activo, este extremo solo sirve como destino de llegada. " +
             "El jugador puede aparecer acá, pero pisarlo no lo teletransporta.")]
    [SerializeField] private bool isDestinationOnly = false;

    [Header("Anti-cheat de recorrido")]
    [Tooltip("Distancia mínima desde el punto de llegada antes de que este extremo vuelva a activarse. " +
             "Usá un valor cercano al largo del pasillo.")]
    [SerializeField, Min(0f)] private float minTravelDistance = 8f;

    [Header("Desvío por iteración")]
    [Tooltip("En esta iteración el jugador es enviado a alternateDestination en lugar del extremo opuesto.")]
    [SerializeField] private int redirectOnIteration = 3;
    [Tooltip("Transform al que se teletransporta en la iteración especial. " +
             "Usá un GameObject vacío posicionado y rotado donde quieras que aparezca el jugador.")]
    [SerializeField] private Transform alternateDestination;


    public static event System.Action<int> OnIterationChanged;
    // Compartido entre los dos extremos
    public static int IterationCount { get; private set; }

    internal float LastTeleportTime = float.NegativeInfinity;

    private Vector3? _arrivalPosition;
    private Transform _playerTransform;

    // ── Unity ─────────────────────────────────────────────────────────

    private void Reset() => GetComponent<Collider>().isTrigger = true;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[CorridorTeleporter] Collider de '{name}' no era trigger. Corregido.", this);
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        if (_arrivalPosition.HasValue && _playerTransform != null)
        {
            if (Vector3.Distance(_playerTransform.position, _arrivalPosition.Value) >= minTravelDistance)
                _arrivalPosition = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestinationOnly) return;

        if (!other.CompareTag(playerTag)) return;
        if (pairedTeleporter == null) return;
        if (!CooldownElapsed()) return;
        if (IsArrivalBlocked(other.transform.position)) return;

        _playerTransform = other.transform;

        IterationCount++;
        Debug.Log($"[CorridorTeleporter] Iteración #{IterationCount}");
        OnIterationChanged?.Invoke(IterationCount);

        // Iteración especial: desviar a destino alternativo
        if (IterationCount == redirectOnIteration && alternateDestination != null)
        {
            TeleportPlayer(other, alternateDestination.position, alternateDestination.rotation);
            RegisterTeleport();
            return;
        }

        // Flujo normal
        TeleportPlayer(
            other,
            CalculateTargetPosition(other.transform.position),
            CalculateTargetRotation(other.transform.rotation)
        );

        pairedTeleporter.RegisterArrival(other.transform.position, other.transform);
        RegisterTeleport();
    }

    // ── Teleport ──────────────────────────────────────────────────────

    /// <summary>
    /// Mueve al jugador desactivando el CharacterController primero para que
    /// Unity no revierta la posición en el mismo frame.
    /// </summary>
    private static void TeleportPlayer(Collider playerCollider, Vector3 position, Quaternion rotation)
    {
        var cc = playerCollider.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        playerCollider.transform.SetPositionAndRotation(position, rotation);
        if (cc != null) cc.enabled = true;
    }

    // ── API interna ───────────────────────────────────────────────────

    internal void RegisterArrival(Vector3 worldPosition, Transform player)
    {
        _arrivalPosition = worldPosition;
        _playerTransform = player;
    }

    private bool IsArrivalBlocked(Vector3 playerPos)
    {
        return _arrivalPosition.HasValue
            && Vector3.Distance(playerPos, _arrivalPosition.Value) < minTravelDistance;
    }

    // ── Cálculos ──────────────────────────────────────────────────────

    private Vector3 CalculateTargetPosition(Vector3 playerWorldPosition)
    {
        Vector3 localOffset = transform.InverseTransformPoint(playerWorldPosition);
        return pairedTeleporter.transform.TransformPoint(localOffset);
    }

    private Quaternion CalculateTargetRotation(Quaternion playerWorldRotation)
    {
        Vector3 targetForward = pairedTeleporter.transform.forward;
        targetForward.y = 0f;
        return Quaternion.LookRotation(targetForward, Vector3.up);
    }

    // ── Cooldown ──────────────────────────────────────────────────────

    private bool CooldownElapsed()
    {
        float now = Time.time;
        return now - LastTeleportTime >= cooldownSeconds
            && now - pairedTeleporter.LastTeleportTime >= cooldownSeconds;
    }

    private void RegisterTeleport()
    {
        float now = Time.time;
        LastTeleportTime = now;
        pairedTeleporter.LastTeleportTime = now;
    }

    // ── Gizmos ────────────────────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color boxColor = isDestinationOnly
            ? new Color(0.8f, 0.4f, 1f, 0.25f)
            : (_arrivalPosition.HasValue
                ? new Color(1f, 0.2f, 0.2f, 0.25f)
                : new Color(0.2f, 0.5f, 1f, 0.20f));

        DrawBox(boxColor);

        if (pairedTeleporter != null)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            Gizmos.DrawLine(transform.position, pairedTeleporter.transform.position);
        }

        if (_arrivalPosition.HasValue)
        {
            Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
            Gizmos.DrawWireSphere(_arrivalPosition.Value, minTravelDistance);
        }

        if (!isDestinationOnly)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
        }
        else
        {
            Gizmos.color = new Color(0.8f, 0.4f, 1f, 1f);
            Gizmos.DrawRay(transform.position, Vector3.up * 1.5f);
        }
    }

    private void OnDrawGizmosSelected() => DrawBox(new Color(0.2f, 0.5f, 1f, 0.50f));

    private void DrawBox(Color color)
    {
        var box = GetComponent<BoxCollider>();
        if (box == null) return;
        Gizmos.color = color;
        var prev = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.matrix = prev;
    }
#endif
}