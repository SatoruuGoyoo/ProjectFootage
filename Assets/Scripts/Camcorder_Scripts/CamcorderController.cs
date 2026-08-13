using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(CamcorderInput))]
[RequireComponent(typeof(VideoRecorder))]
[RequireComponent(typeof(SpatialAudioRecorder))]
[RequireComponent(typeof(CamcorderStorage))]
[RequireComponent(typeof(CamcorderMotor))]
public class CamcorderController : MonoBehaviour
{
    public static bool LiftInputBlocked = false;
    public static bool RecordInputBlocked = false;

    [Header("Setup")]
    [SerializeField] private GameObject camcorderVisual;
    [SerializeField] private PlayerMotor playerMotor;
    [SerializeField] private Camera recordingCamera;

    [Header("Timing/Runtime")]
    [SerializeField] private float prepareTimer = 0f;
    [SerializeField] private float recordTimer = 0f;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private int prepareDuration = 1;
    [SerializeField] private int recordDuration = 5;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("FMOD")]
    [SerializeField] private EventReference toggleEvent;

    public CamcorderMode CurrentCamMode => _currentCamMode;

    private CamcorderMode _currentCamMode = CamcorderMode.Idle;
    private PlayerMode _currentPlayerMode = PlayerMode.ExplorationMode;

    private VideoRecorder _videoRecorder;
    private SpatialAudioRecorder _spatialAudioRecorder;
    private CamcorderStorage _storage;
    private CamcorderInput _input;
    private CamcorderMotor _motor;

    private bool _isCameraUp = false;
    private RecordingSession _activeSession;
    private float _recordingTimer;

    private void Awake()
    {
        _input = GetComponent<CamcorderInput>();
        _videoRecorder = GetComponent<VideoRecorder>();
        _spatialAudioRecorder = GetComponent<SpatialAudioRecorder>();
        _storage = GetComponent<CamcorderStorage>();
        _motor = GetComponent<CamcorderMotor>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
        GameEvents.OnRecordableEventCompleted += OnRecordableEventCompleted;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
        GameEvents.OnRecordableEventCompleted -= OnRecordableEventCompleted;
    }

    private void Start()
    {
        camcorderVisual.SetActive(false);
        if (recordingCamera != null) recordingCamera.enabled = false;
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        _currentPlayerMode = newMode;

        if ((newMode == PlayerMode.MenuCameraMode || newMode == PlayerMode.InteractionMode) && _isCameraUp)
        {
            _isCameraUp = false;
            camcorderVisual.SetActive(false);
            if (recordingCamera != null) recordingCamera.enabled = false;
            _currentCamMode = CamcorderMode.Idle;
            prepareTimer = 0f;
            recordTimer = 0f;
        }
    }
    private void OnRecordableEventCompleted(string eventId)
    {
        if (_currentCamMode == CamcorderMode.Recording)
            StopRecording();
    }

    private void Update()
    {
        if (_currentPlayerMode == PlayerMode.MenuCameraMode) return;
        if (_currentPlayerMode == PlayerMode.InteractionMode) return;

        if (_input.LiftCamera && !LiftInputBlocked) ToggleCamera();

        if (_isCameraUp && _currentCamMode != CamcorderMode.Recording)
        {
            _motor.Tilt(_input.RecordingTilt * mouseSensitivity);
            playerMotor.RotateDirect(_input.RecordingRotate * mouseSensitivity * _motor.RotateSpeed * Time.deltaTime);
        }

        HandleCamcorderState();
    }

    private void ToggleCamera()
    {
        if (_currentPlayerMode == PlayerMode.MenuCameraMode) return;
        if (_currentPlayerMode == PlayerMode.InteractionMode) return;
        if (_currentCamMode == CamcorderMode.Recording) return;

        _isCameraUp = !_isCameraUp;
        camcorderVisual.SetActive(_isCameraUp);
        if (recordingCamera != null) recordingCamera.enabled = _isCameraUp;

        FMODManager.Instance.PlayOneShot(toggleEvent, transform.position);

        if (_isCameraUp)
            GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
        else
        {
            _motor.ResetRotation();
            GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        }
    }

    private void HandleCamcorderState()
    {
        if (!_isCameraUp) return;

        switch (_currentCamMode)
        {
            case CamcorderMode.Idle:
                if (_input.StartedRecording && !RecordInputBlocked)
                    _currentCamMode = CamcorderMode.Preparing;
                break;

            case CamcorderMode.Preparing:
                prepareTimer += Time.deltaTime;
                if (_input.IsRecordingReleased)
                {
                    _currentCamMode = CamcorderMode.Idle;
                    prepareTimer = 0f;
                }
                else if (prepareTimer >= prepareDuration)
                {
                    StartRecording();
                    prepareTimer = 0f;
                }
                break;

            case CamcorderMode.Recording:
                _recordingTimer += Time.deltaTime;
                recordTimer += Time.deltaTime;

                _motor.Tilt(_input.RecordingTilt * mouseSensitivity);
                playerMotor.RotateDirect(_input.RecordingRotate * mouseSensitivity * _motor.RotateSpeed * Time.deltaTime);

                if (_input.IsRecordingReleased || recordTimer >= recordDuration)
                    StopRecording();
                break;
        }
    }

    private void StartRecording()
    {
        _activeSession = new RecordingSession();
        _recordingTimer = 0f;
        recordTimer = 0f;

        _videoRecorder.StartRecording(_activeSession);
        _spatialAudioRecorder.StartRecording(_activeSession);

        _currentCamMode = CamcorderMode.Recording;
        GameEvents.PlayerModeChanged(PlayerMode.RecordingMode);
        GameEvents.RecordingStarted(_activeSession);
    }

    private void StopRecording()
    {
        _videoRecorder.StopRecording();
        _spatialAudioRecorder.StopRecording();

        _activeSession.Complete(_recordingTimer);
        _storage.AddRecording(_activeSession);
        _activeSession = null;

        recordTimer = 0f;
        _recordingTimer = 0f;
        _currentCamMode = CamcorderMode.Idle;
        _motor.ResetRotation();
        GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
        GameEvents.RecordingStopped();
    }

    public void ForceRaiseCamera()
    {
        if (!_isCameraUp)
            ToggleCamera();
    }

    public void ForceLowerCamera()
    {
        if (_isCameraUp && _currentCamMode != CamcorderMode.Recording)
            ToggleCamera();
    }
}