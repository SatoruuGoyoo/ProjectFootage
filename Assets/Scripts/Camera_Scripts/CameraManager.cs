using UnityEngine;

public class CameraManager : MonoBehaviour
{
    private Camera currentCamera;

    public void SetCamera(Camera newCamera)
    {
        if (currentCamera == newCamera) return;

        if (currentCamera != null)
            currentCamera.gameObject.SetActive(false);

        currentCamera = newCamera;

        if (currentCamera != null)
            currentCamera.gameObject.SetActive(true);
    }
}