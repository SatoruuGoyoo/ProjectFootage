using UnityEngine;
using UnityEngine.InputSystem;

public class CamcorderInput : MonoBehaviour
{
    public bool LiftCamera { get; private set; }
    public float TiltCamera { get; private set; }
    public bool IsPreparingRecording { get; private set; }
    public bool IsRecordingReleased { get; private set; }
    public bool StartedRecording { get; private set; }

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

        TiltCamera = actions.Camera.TiltCamera.ReadValue<float>();

        IsPreparingRecording = actions.Camera.@StartRecordingRecording.IsPressed(); // StartRecording/Recording 

        StartedRecording = actions.Camera.@StartRecordingRecording.WasPressedThisFrame(); // StartRecording/Recording

        IsRecordingReleased = actions.Camera.@StartRecordingRecording.WasReleasedThisFrame(); // StartRecording/Recording

    }
}