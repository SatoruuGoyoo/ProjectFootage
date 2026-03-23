using Unity.VisualScripting;
using UnityEngine;

public class CamcorderController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject camcorderVisual;

    private CamcorderInput input;
    private bool isCameraUp = false;

    private void Awake()
    {
        input = GetComponent<CamcorderInput>();
    }

    private void Start()
    {
        camcorderVisual.SetActive(false);
    }

    private void Update()
    {
        if (input.LiftCamera)
        {
            ToggleCamera();
        }

        if (isCameraUp)
            GetComponent<CamcorderMotor>().Tilt(input.TiltCamera);
    }

    private void ToggleCamera()
    {
        isCameraUp = !isCameraUp;
        camcorderVisual.SetActive(isCameraUp);

        if (isCameraUp)
            GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
        else
            GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

}