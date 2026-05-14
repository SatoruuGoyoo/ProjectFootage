using UnityEngine;

public class TriggerActivator : MonoBehaviour
{
    [Header("Trigger a activar")]
    public GameObject triggerB;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerB != null)
            triggerB.SetActive(true);
        gameObject.SetActive(false);
    }
}