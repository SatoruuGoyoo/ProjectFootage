using FMODUnity;
using UnityEngine;

public class CamcorderController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject camcorderVisual;
    public PlayerMotor playerMotor;

    [Header("Timing/Runtime")]
    [SerializeField] private float prepareTimer = 0f;
    [SerializeField] private float recordTimer = 0f;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private int prepareDuration = 1;
    [SerializeField] private int recordDuration = 5;
    [SerializeField] private float mouseSensitivity = 2f;

    [Header("FMOD")]
    [SerializeField] private EventReference toggleEvent;
    [SerializeField] private EventReference ambientRecordingEvent; // el audio del mundo que se graba

    public CamcorderMode CurrentCamMode => _currentCamMode;

    private CamcorderMode _currentCamMode = CamcorderMode.Idle;
    private PlayerMode _currentPlayerMode = PlayerMode.ExplorationMode;

    // Nuevos recorders separados
    private VideoRecorder _videoRecorder;
    private AudioRecorder _audioRecorder;
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
        _audioRecorder = GetComponent<AudioRecorder>();
        _storage = GetComponent<CamcorderStorage>();
        _motor = GetComponent<CamcorderMotor>();
    }

    private void OnEnable() => GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    private void OnDisable() => GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;

    private void Start() => camcorderVisual.SetActive(false);

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        _currentPlayerMode = newMode;

        if (newMode == PlayerMode.MenuCameraMode && _isCameraUp)
        {
            _isCameraUp = false;
            camcorderVisual.SetActive(false);
            _currentCamMode = CamcorderMode.Idle;
            prepareTimer = 0f;
            recordTimer = 0f;
        }
    }

    private void Update()
    {
        if (_currentPlayerMode == PlayerMode.MenuCameraMode) return;

        if (_input.LiftCamera) ToggleCamera();

        if (_isCameraUp && _currentCamMode != CamcorderMode.Recording)
        {
            _motor.Tilt(_input.RecordingTilt * mouseSensitivity);
            playerMotor.RotateDirect(_input.RecordingRotate * mouseSensitivity * _motor.rotateSpeed * Time.deltaTime);
        }

        HandleCamcorderState();
    }

    private void ToggleCamera()
    {
        if (_currentPlayerMode == PlayerMode.MenuCameraMode) return;
        if (_currentCamMode == CamcorderMode.Recording) return;

        _isCameraUp = !_isCameraUp;
        camcorderVisual.SetActive(_isCameraUp);

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
                if (_input.StartedRecording)
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
                playerMotor.RotateDirect(_input.RecordingRotate * mouseSensitivity * _motor.rotateSpeed * Time.deltaTime);

                if (_input.IsRecordingReleased || recordTimer >= recordDuration)
                    StopRecording();
                break;
        }
    }

    // ── Grabación ──────────────────────────────────────────────

    private void StartRecording()
    {
        // Creamos la sesión acá — el Controller es el dueño
        _activeSession = new RecordingSession();
        _recordingTimer = 0f;
        recordTimer = 0f;

        _videoRecorder.StartRecording(_activeSession);
        _audioRecorder.StartRecording(_activeSession, ambientRecordingEvent);

        _currentCamMode = CamcorderMode.Recording;
        GameEvents.PlayerModeChanged(PlayerMode.RecordingMode);
        GameEvents.RecordingStarted();
    }

    private void StopRecording()
    {
        _videoRecorder.StopRecording();
        _audioRecorder.StopRecording();

        // El Controller es el único que llama Complete()
        // porque es el único que sabe que AMBOS recorders terminaron
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
}