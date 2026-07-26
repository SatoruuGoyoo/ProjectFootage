using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Volumen manual que agrupa renderers para culling por zonas. Cada zona
/// representa un cuarto/área; su lista de renderers se cachea una sola vez
/// y se prende/apaga en bloque (Renderer.enabled, nunca SetActive) para que
/// el toggle sea prácticamente gratis.
///
/// 'visibleNeighbors' es la PVS manual: qué otras zonas deberían verse
/// cuando esta zona está activa (por ejemplo, el cuarto del otro lado de
/// una puerta). No hay recursión automática — si querés ver a través de
/// dos puertas seguidas, agregá esa zona también a la lista.
///
/// Si tiene un Collider marcado como Trigger y 'triggeredByPlayer' activo,
/// además dispara el cambio de zona del jugador (lo usa el camcorder). Las
/// zonas que solo alimentan cámaras fijas no necesitan trigger.
/// </summary>
[DisallowMultipleComponent]
public sealed class OcclusionZone : MonoBehaviour
{
    [Header("Contenido")]
    [SerializeField] private Renderer[] renderers = System.Array.Empty<Renderer>();

    [Header("PVS manual")]
    [SerializeField] private OcclusionZone[] visibleNeighbors = System.Array.Empty<OcclusionZone>();

    [Header("Trigger de jugador (opcional)")]
    [SerializeField] private bool triggeredByPlayer;
    [SerializeField] private string playerTag = "Player";

    public IReadOnlyList<OcclusionZone> VisibleNeighbors => visibleNeighbors;
    public bool IsVisible { get; private set; } = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggeredByPlayer || !other.CompareTag(playerTag)) return;
        OcclusionCullingManager.Instance.SetPlayerZone(this);
    }

    public void SetVisible(bool visible)
    {
        if (IsVisible == visible) return;
        IsVisible = visible;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = visible;
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Recolectar renderers de los hijos")]
    private void CollectChildRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);
        UnityEditor.EditorUtility.SetDirty(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < visibleNeighbors.Length; i++)
        {
            if (visibleNeighbors[i] != null)
                Gizmos.DrawLine(transform.position, visibleNeighbors[i].transform.position);
        }
    }
#endif
}