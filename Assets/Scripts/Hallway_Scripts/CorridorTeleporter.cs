using UnityEngine;

/// <summary>
/// Colocá este componente en dos GameObjects vacíos, uno en cada extremo
/// del pasillo. Cada uno necesita un BoxCollider con isTrigger = true.
/// Vinculá el extremo opuesto en el campo Paired Teleporter.
///
/// ORIENTACIÓN IMPORTANTE:
/// El eje Z (flecha azul) de cada teleportador debe apuntar
/// hacia el interior del pasillo al que pertenece.
/// Así el player queda alineado automáticamente al teleportar.
/// </summary>
[RequireComponent(typeof(Collider))]
public sealed class CorridorTeleporter : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private CorridorTeleporter pairedTeleporter;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Range(0.05f, 2f)] private float cooldownSeconds = 0.3f;

    internal float LastTeleportTime = float.NegativeInfinity;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning($"[CorridorTeleporter] Collider de '{name}' no era trigger. Corregido.", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))    return;
        if (pairedTeleporter == null)        return;
        if (!CooldownElapsed())              return;

        var mover = other.GetComponent<IPlayerMover>();
        if (mover == null)
        {
            Debug.LogWarning($"[CorridorTeleporter] '{other.name}' no tiene PlayerMover.", other);
            return;
        }

        Transform player = other.transform;
        mover.MoveTo(
            CalculateTargetPosition(player.position),
            CalculateTargetRotation(player.rotation)
        );

        RegisterTeleport();
    }

    // ── Cálculos ──────────────────────────────────────────────────────

    /// <summary>
    /// Convierte la posición del jugador al espacio local de este
    /// teleportador y la expresa en el espacio mundial del par.
    /// Preserva el desplazamiento lateral dentro del pasillo.
    /// </summary>
    private Vector3 CalculateTargetPosition(Vector3 playerWorldPosition)
    {
        Vector3 localOffset = transform.InverseTransformPoint(playerWorldPosition);
        return pairedTeleporter.transform.TransformPoint(localOffset);
    }

    /// <summary>
    /// Calcula la rotación que el player debe tener al salir del
    /// teleportador destino, conservando el ángulo relativo que tenía
    /// respecto al teleportador origen.
    ///
    /// Ejemplo: si el player iba recto dentro del pasillo A (alineado
    /// con el Z del teleportador A), al salir queda alineado con el Z
    /// del teleportador B, es decir, recto dentro del pasillo B.
    /// </summary>
    private Quaternion CalculateTargetRotation(Quaternion playerWorldRotation)
    {
        // Mirar SIEMPRE hacia donde apunta el TP destino
        Vector3 targetForward = pairedTeleporter.transform.forward;

        // Solo conservar inclinación en Y (sin pitch/roll)
        targetForward.y = 0f;

        return Quaternion.LookRotation(targetForward, Vector3.up);
    }
    // ── Cooldown ──────────────────────────────────────────────────────

    private bool CooldownElapsed()
    {
        float now = Time.time;
        return now - LastTeleportTime                  >= cooldownSeconds
            && now - pairedTeleporter.LastTeleportTime >= cooldownSeconds;
    }

    private void RegisterTeleport()
    {
        float now             = Time.time;
        LastTeleportTime                  = now;
        pairedTeleporter.LastTeleportTime = now;
    }

    // ── Gizmos (solo editor) ──────────────────────────────────────────

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DrawBox(new Color(0.2f, 0.5f, 1f, 0.20f));
        if (pairedTeleporter != null)
        {
            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            Gizmos.DrawLine(transform.position, pairedTeleporter.transform.position);
        }

        // Flecha que indica el "hacia adentro" del pasillo (eje Z del teleportador)
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);
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
