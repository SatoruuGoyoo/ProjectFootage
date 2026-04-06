using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private Camera currentCamera;
    public Camera ActiveCamera => currentCamera;

    private void Awake()
    {
        Instance = this;
    }

    public void SetCamera(Camera newCam)
    {
        if (currentCamera == newCam) return;
        if (currentCamera != null) currentCamera.gameObject.SetActive(false);
        currentCamera = newCam;
        if (currentCamera != null) currentCamera.gameObject.SetActive(true);
    }
}