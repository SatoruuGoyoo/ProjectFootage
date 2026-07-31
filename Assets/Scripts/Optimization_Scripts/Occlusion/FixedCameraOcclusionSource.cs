using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PVS manual de una cámara fija: qué zonas puede llegar a ver desde su
/// posición, sin importar hacia dónde esté rotando (Static/LookAt/Follow).
/// Como la posición de la cámara no cambia, esta lista se define una sola
/// vez a mano; el frustum culling normal de Unity ya se encarga de la
/// parte de "hacia dónde mira" cuando la cámara sigue al jugador.
///
/// Además de las zonas, 'Hidden By Occluders' es una lista de objetos
/// puntuales que están dentro de una zona visible pero tapados por otro
/// objeto desde esta posición exacta (un sillón que tapa una silla). Se
/// llena con el botón "Bakear objetos ocultos", que tira un rayo desde la
/// cámara a cada renderer candidato una sola vez en el editor — no hay
/// costo de esto en juego. Podés editar la lista a mano después del bake.
///
/// Colocar junto al FixedCameraController correspondiente y llamar a
/// Activate() desde donde el CameraManager active esa cámara.
/// </summary>
public sealed class FixedCameraOcclusionSource : MonoBehaviour
{
    [SerializeField] private OcclusionZone[] visibleZones = System.Array.Empty<OcclusionZone>();

    [Tooltip("Cuántos saltos extra de vecinos sumar además de lo listado en Visible Zones. " +
        "Subilo (1-2) en cámaras LookAt para que el barrido no muestre huecos cerca de los bordes de rotación.")]
    [Min(0)][SerializeField] private int margin = 1;

    [Header("Oclusión por objeto (opcional)")]
    [Tooltip("Qué capas cuentan como algo que puede tapar a otro objeto (paredes, muebles grandes).")]
    [SerializeField] private LayerMask occluderMask;
    [SerializeField] private Renderer[] hiddenByOccluders = System.Array.Empty<Renderer>();

    public void Activate()
    {
        OcclusionCullingManager.Instance.SetFixedCameraZones(visibleZones, hiddenByOccluders, margin);
    }

#if UNITY_EDITOR
    [ContextMenu("Bakear objetos ocultos")]
    private void BakeOccluders()
    {
        var hidden = new List<Renderer>();
        var seen = new HashSet<Renderer>();
        Vector3 origin = transform.position;

        for (int z = 0; z < visibleZones.Length; z++)
        {
            if (visibleZones[z] == null) continue;

            IReadOnlyList<Renderer> zoneRenderers = visibleZones[z].Renderers;
            for (int r = 0; r < zoneRenderers.Count; r++)
            {
                Renderer candidate = zoneRenderers[r];
                if (candidate == null || !seen.Add(candidate)) continue;

                Vector3 target = candidate.bounds.center;
                if (Physics.Linecast(origin, target, out RaycastHit hit, occluderMask) && hit.collider.GetComponentInParent<Renderer>() != candidate)
                    hidden.Add(candidate);
            }
        }

        hiddenByOccluders = hidden.ToArray();
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[{nameof(FixedCameraOcclusionSource)}] '{name}': {hidden.Count} objetos marcados como ocultos por otros objetos.", this);
    }
#endif
}