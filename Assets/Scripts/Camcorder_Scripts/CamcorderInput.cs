using UnityEngine;
using UnityEngine.InputSystem;

public class CamcorderInput : MonoBehaviour
{
    public bool LiftCamera { get; private set; }

    private PlayerInputActions actions;

    private void Awake()
    {
        actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        actions.Exploration.Enable();
        actions.Camera.Enable();
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
        actions.Camera.Disable();
    }

    private void Update()
    {
        LiftCamera = actions.Exploration.LiftCamera.WasPressedThisFrame() ||
                     actions.Camera.PutDownCamera.WasPressedThisFrame();

        if (LiftCamera)
            Debug.Log("LiftCamera detectado");
    }
}