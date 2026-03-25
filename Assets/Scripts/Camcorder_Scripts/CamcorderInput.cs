using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CamcorderInput : MonoBehaviour
{
    public bool LiftCamera { get; private set; }
    public float TiltCamera { get; private set; }
    public bool IsPreparingRecording { get; private set; }
    public bool IsRecordingReleased { get; private set; }
    public bool StartedRecording { get; private set; }
    public bool OpenCloseMenu { get; private set; }
    public float NavigateMenu { get; private set; }
    public bool PlayPauseRecording { get; private set; }
    public bool RewindRecording { get; private set; }
    public bool FastForwardRecording { get; private set; }
    public float RotateRecording { get; private set; }




    private PlayerInputActions actions;

    private void Awake()
    {
        actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        actions.Exploration.Enable();
        actions.Camera.Enable();
        actions.MenuCamera.Enable();
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
        actions.Camera.Disable();
        actions.MenuCamera.Disable();
    }

    private void Update()
    {
        // Camcorder Mode Inputs
        LiftCamera = actions.Exploration.LiftCamera.WasPressedThisFrame() ||
                     actions.Camera.PutDownCamera.WasPressedThisFrame();

        TiltCamera = actions.Camera.TiltCamera.ReadValue<float>();
        IsPreparingRecording = actions.Camera.@StartRecordingRecording.IsPressed(); // StartRecording/Recording 
        StartedRecording = actions.Camera.@StartRecordingRecording.WasPressedThisFrame(); // StartRecording/Recording
        IsRecordingReleased = actions.Camera.@StartRecordingRecording.WasReleasedThisFrame(); // StartRecording/Recording

        // Menu Camera Mode Inputs
        OpenCloseMenu = actions.MenuCamera.OpenClose.WasPressedThisFrame();
        NavigateMenu = actions.MenuCamera.Navigate.ReadValue<float>();
        PlayPauseRecording = actions.MenuCamera.PlayPause.WasPressedThisFrame();
        RewindRecording = actions.MenuCamera.Rewind.IsPressed();
        FastForwardRecording = actions.MenuCamera.FastForward.IsPressed();
        RotateRecording = actions.MenuCamera.Rotate.ReadValue<float>();



    }
}