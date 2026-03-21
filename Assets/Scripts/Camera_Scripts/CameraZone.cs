using UnityEngine;

public class CameraZone : MonoBehaviour
{
    public Camera zoneCamera;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CameraManager manager = FindAnyObjectByType<CameraManager>();

            if (manager != null && zoneCamera != null)
            {
                manager.SetCamera(zoneCamera);
            }
        }
    }
}