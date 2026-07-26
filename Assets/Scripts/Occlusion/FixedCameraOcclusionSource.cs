using UnityEngine;

/// <summary>
/// PVS manual de una cámara fija: qué zonas puede llegar a ver desde su
/// posición, sin importar hacia dónde esté rotando (Static/LookAt/Follow).
/// Como la posición de la cámara no cambia, esta lista se define una sola
/// vez a mano; el frustum culling normal de Unity ya se encarga de la
/// parte de "hacia dónde mira" cuando la cámara sigue al jugador.
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

    public void Activate()
    {
        OcclusionCullingManager.Instance.SetFixedCameraZones(visibleZones, margin);
    }
}