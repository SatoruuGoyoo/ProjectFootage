using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform destination;

    [Header("Tag Filter")]
    public string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag))
            return;

        if (destination == null)
        {
            Debug.LogWarning("TeleportTrigger: No hay destino asignado.", this);
            return;
        }

        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            other.transform.position = destination.position;
            cc.enabled = true;
        }
        else
        {
            other.transform.position = destination.position;
        }

        // other.transform.rotation = destination.rotation;
    }
}