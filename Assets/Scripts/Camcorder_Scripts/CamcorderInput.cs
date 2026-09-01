using UnityEngine;
using UnityEngine.InputSystem;

public class CamcorderInput : MonoBehaviour
{
    // Camera
    public bool LiftCamera { get; private set; }
    public float RecordingRotate { get; private set; }
    public float RecordingTilt { get; private set; }
    public float TiltCamera { get; private set; }
    public bool IsPreparingRecording { get; private set; }
    public bool IsRecordingReleased { get; private set; }
    public bool StartedRecording { get; private set; }
    // Menu — pressed this frame
    public bool OpenCloseMenu { get; private set; }
    public bool NavigateRight { get; private set; }
    public bool NavigateLeft { get; private set; }
    public bool NavigateUp { get; private set; }
    public bool NavigateDown { get; private set; }
    public bool PlayPauseRecording { get; private set; }
    public bool RewindRecording { get; private set; }
    public bool FastForwardRecording { get; private set; }
    public float RotateRecording { get; private set; }
    public bool DiscardRecording { get; private set; }
    public bool StopRecording { get; private set; }
    // Menu — held
    public bool OpenCloseMenuHeld { get; private set; }
    public bool NavigateRightHeld { get; private set; }
    public bool NavigateLeftHeld { get; private set; }
    public bool NavigateUpHeld { get; private set; }
    public bool NavigateDownHeld { get; private set; }
    public bool PlayPauseRecordingHeld { get; private set; }
    public bool DiscardRecordingHeld { get; private set; }
    public bool StopRecordingHeld { get; private set; }

    private PlayerInputActions actions;
    private bool _isPaused;

    private void Awake() => actions = new PlayerInputActions();

    private void OnEnable()
    {
        actions.Exploration.Enable();
        actions.Camera.Enable();
        actions.MenuCamera.Enable();
        GameEvents.OnPauseChanged += OnPauseChanged;
    }

    private void OnDisable()
    {
        actions.Exploration.Disable();
        actions.Camera.Disable();
        actions.MenuCamera.Disable();
        GameEvents.OnPauseChanged -= OnPauseChanged;
    }

    private void OnPauseChanged(bool paused)
    {
        _isPaused = paused;
        if (paused) ClearAll();
    }

    private void Update()
    {
        if (InputLock.AllBlocked || _isPaused)
        {
            ClearAll();
            return;
        }

        LiftCamera = actions.Exploration.LiftCamera.WasPressedThisFrame() ||
                     actions.Camera.PutDownCamera.WasPressedThisFrame();
        RecordingRotate = actions.Camera.RecordingRotate.ReadValue<float>();
        RecordingTilt = actions.Camera.RecordingTilt.ReadValue<float>();
        TiltCamera = actions.Camera.TiltCamera.ReadValue<float>();
        IsPreparingRecording = actions.Camera.StartRecordingRecording.IsPressed();
        StartedRecording = actions.Camera.StartRecordingRecording.WasPressedThisFrame();
        IsRecordingReleased = actions.Camera.StartRecordingRecording.WasReleasedThisFrame();

        OpenCloseMenu = actions.MenuCamera.OpenClose.WasPressedThisFrame();

        Vector2 nav = actions.MenuCamera.Navigate.ReadValue<Vector2>();
        bool navPressedThisFrame = actions.MenuCamera.Navigate.WasPressedThisFrame();

        NavigateRight = navPressedThisFrame && nav.x > 0.5f;
        NavigateLeft = navPressedThisFrame && nav.x < -0.5f;
        NavigateUp = navPressedThisFrame && nav.y > 0.5f;
        NavigateDown = navPressedThisFrame && nav.y < -0.5f;

        PlayPauseRecording = actions.MenuCamera.PlayPause.WasPressedThisFrame();
        RewindRecording = actions.MenuCamera.Rewind.IsPressed();
        FastForwardRecording = actions.MenuCamera.FastForward.IsPressed();
        RotateRecording = actions.MenuCamera.Rotate.ReadValue<float>();
        DiscardRecording = actions.MenuCamera.Discard.WasPressedThisFrame();
        StopRecording = actions.MenuCamera.Stop.WasPressedThisFrame();

        OpenCloseMenuHeld = actions.MenuCamera.OpenClose.IsPressed();

        bool navHeld = actions.MenuCamera.Navigate.IsPressed();
        NavigateRightHeld = navHeld && nav.x > 0.5f;
        NavigateLeftHeld = navHeld && nav.x < -0.5f;
        NavigateUpHeld = navHeld && nav.y > 0.5f;
        NavigateDownHeld = navHeld && nav.y < -0.5f;

        PlayPauseRecordingHeld = actions.MenuCamera.PlayPause.IsPressed();
        DiscardRecordingHeld = actions.MenuCamera.Discard.IsPressed();
        StopRecordingHeld = actions.MenuCamera.Stop.IsPressed();
    }

    private void ClearAll()
    {
        LiftCamera = false;
        RecordingRotate = 0f;
        RecordingTilt = 0f;
        TiltCamera = 0f;
        IsPreparingRecording = false;
        StartedRecording = false;
        IsRecordingReleased = false;
        OpenCloseMenu = false;
        NavigateRight = false;
        NavigateLeft = false;
        NavigateUp = false;
        NavigateDown = false;
        PlayPauseRecording = false;
        RewindRecording = false;
        FastForwardRecording = false;
        RotateRecording = 0f;
        DiscardRecording = false;
        StopRecording = false;
        OpenCloseMenuHeld = false;
        NavigateRightHeld = false;
        NavigateLeftHeld = false;
        NavigateUpHeld = false;
        NavigateDownHeld = false;
        PlayPauseRecordingHeld = false;
        DiscardRecordingHeld = false;
        StopRecordingHeld = false;
    }
}