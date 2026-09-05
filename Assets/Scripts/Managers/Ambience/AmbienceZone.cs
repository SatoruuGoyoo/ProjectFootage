using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AmbienceZone : MonoBehaviour
{
    [SerializeField] private EventReference zoneAmbient;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            AmbienceManager.Instance.SetAmbience(zoneAmbient);
    }
}