using UnityEngine;

/// <summary>
/// Colocá este componente en cualquier puzzle nuevo.
/// Llamá NotifyCompleted() desde el script del puzzle cuando esté resuelto,
/// o conéctalo desde un UnityEvent / botón en el Inspector.
///
/// En el HorrorEventManager, usá triggerType = PuzzleCompleted
/// y escribí el mismo puzzleId en el campo puzzleIdFilter de la entrada.
/// Si puzzleIdFilter está vacío, la entrada se dispara con CUALQUIER puzzle.
/// </summary>
public class PuzzleCompletedNotifier : MonoBehaviour
{
    [Tooltip("ID único de este puzzle. Debe coincidir con puzzleIdFilter en HorrorEventManager.\nEjemplos: 'DoorPuzzle', 'ClockRoom', 'MirrorPuzzle'")]
    public string puzzleId = "MiPuzzle";

    /// <summary>
    /// Llamá este método cuando el puzzle se complete.
    /// Compatible con UnityEvents del Inspector.
    /// </summary>
    public void NotifyCompleted()
    {
        if (string.IsNullOrEmpty(puzzleId))
        {
            Debug.LogWarning($"[PuzzleCompletedNotifier] puzzleId está vacío en '{gameObject.name}'. Asignale un ID único.");
            return;
        }

        Debug.Log($"[PuzzleCompletedNotifier] Puzzle completado: '{puzzleId}'");
        GameEvents.PuzzleCompleted(puzzleId);
    }
}
