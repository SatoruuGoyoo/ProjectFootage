using UnityEngine;

/// <summary>
/// One-shot trigger that permanently stops a RainController when Player enters it.
/// </summary>
public class RainStopTrigger : MonoBehaviour
{
    [SerializeField] private RainController rainController;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableAfterTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (rainController != null)
            rainController.StopRain();
        else
            Debug.LogWarning($"{name}: assign a RainController.", this);

        if (disableAfterTrigger)
            gameObject.SetActive(false);
    }
}
