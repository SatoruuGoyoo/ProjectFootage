using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Volumen manual que agrupa el contenido de un cuarto para culling por
/// zonas: renderers, colliders, luces y partículas se cachean una sola vez
/// y se prenden/apagan en bloque (nunca SetActive) para que el toggle sea
/// prácticamente gratis. Ocultar también colliders y luces evita que la
/// física siga simulando choques o que una luz siga tirando sombra por un
/// cuarto que nadie está mirando.
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
    [SerializeField] private Collider[] colliders = System.Array.Empty<Collider>();
    [SerializeField] private Light[] lights = System.Array.Empty<Light>();
    [SerializeField] private ParticleSystem[] particles = System.Array.Empty<ParticleSystem>();

    [Header("PVS manual")]
    [SerializeField] private OcclusionZone[] visibleNeighbors = System.Array.Empty<OcclusionZone>();

    [Header("Trigger de jugador (opcional)")]
    [SerializeField] private bool triggeredByPlayer;
    [SerializeField] private string playerTag = "Player";

    public IReadOnlyList<OcclusionZone> VisibleNeighbors => visibleNeighbors;
    public IReadOnlyList<Renderer> Renderers => renderers;
    public bool IsVisible { get; private set; } = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!triggeredByPlayer || !other.CompareTag(playerTag)) return;
        OcclusionCullingManager.Instance.SetPlayerZone(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!triggeredByPlayer || !other.CompareTag(playerTag)) return;
        OcclusionCullingManager.Instance.ClearPlayerZoneIfCurrent(this);
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

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].enabled = visible;
        }

        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null)
                lights[i].enabled = visible;
        }

        for (int i = 0; i < particles.Length; i++)
        {
            if (particles[i] == null) continue;

            if (visible) particles[i].Play();
            else particles[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Recolectar contenido de los hijos")]
    private void CollectChildContent()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        var childColliders = new List<Collider>(GetComponentsInChildren<Collider>(includeInactive: true));
        childColliders.RemoveAll(c => c.gameObject == gameObject);
        colliders = childColliders.ToArray();

        lights = GetComponentsInChildren<Light>(includeInactive: true);
        particles = GetComponentsInChildren<ParticleSystem>(includeInactive: true);

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