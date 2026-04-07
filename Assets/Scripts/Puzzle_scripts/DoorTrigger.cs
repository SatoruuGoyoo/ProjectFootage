using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DoorTrigger : MonoBehaviour
{
    [Header("Identificación")]
    [Tooltip("Índice de esta puerta (0 a 4). Asígnalo manualmente en Inspector.")]
    [Range(0, 5)]
    public int doorIndex = 0;

    [Header("Debug")]
    [Tooltip("Tag del objeto player para filtrar el trigger.")]
    public string playerTag = "Player";

    void Start()
    {
        // Asegura que el collider sea trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[DoorTrigger] Puerta {doorIndex + 1}: Collider convertido a Trigger automáticamente.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        Debug.Log($"[DoorTrigger] Player entró en puerta {doorIndex + 1}.");
        PuzzleManager.Instance?.OnDoorEntered(doorIndex);
    }
}
