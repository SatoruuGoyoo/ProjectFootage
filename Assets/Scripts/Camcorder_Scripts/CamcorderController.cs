using UnityEngine;

public class CamcorderController : MonoBehaviour
{
    [Header("Setup")]
    public GameObject camcorderVisual;
    public PlayerMotor playerMotor;
    private CamcorderMotor camcorderMotor;

    [Header("Timing/Runtime")]
    [SerializeField] private float prepareTimer = 0f;
    [SerializeField] private float recordTimer = 0f;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private int prepareDuration = 1;
    [SerializeField] private int recordDuration = 5;
    [SerializeField] private float mouseSensitivity = 2f;

    public CamcorderMode CurrentCamMode => currentCamMode;
    private CamcorderMode currentCamMode = CamcorderMode.Idle;
    private PlayerMode currentPlayerMode = PlayerMode.ExplorationMode;
    private CamcorderRecorder recorder;
    private CamcorderStorage storage;
    private CamcorderInput input;
    private bool isCameraUp = false;

    private void Awake()
    {
        input = GetComponent<CamcorderInput>();
        recorder = GetComponent<CamcorderRecorder>();
        storage = GetComponent<CamcorderStorage>();
        camcorderMotor = GetComponent<CamcorderMotor>();
    }

    private void OnEnable()
    {
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        currentPlayerMode = newMode;

        if (newMode == PlayerMode.MenuCameraMode && isCameraUp)
        {
            isCameraUp = false;
            camcorderVisual.SetActive(false);
            currentCamMode = CamcorderMode.Idle;
            prepareTimer = 0f;
            recordTimer = 0f;
        }
    }

    private void Start()
    {
        camcorderVisual.SetActive(false);
    }

    private void Update()
    {
        if (currentPlayerMode == PlayerMode.MenuCameraMode) return;

        if (input.LiftCamera)
            ToggleCamera();

        // Idle/Preparing: solo tilt (ambos schemes)
        if (isCameraUp && currentCamMode != CamcorderMode.Recording)
            camcorderMotor.Tilt(input.TiltCamera);

        HandleCamcorderState();
    }

    private void ToggleCamera()
    {
        if (currentPlayerMode == PlayerMode.MenuCameraMode) return;
        if (currentCamMode == CamcorderMode.Recording) return;

        isCameraUp = !isCameraUp;
        camcorderVisual.SetActive(isCameraUp);

        if (isCameraUp)
            GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
        else
        {
            camcorderMotor.ResetRotation();
            GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        }
    }

    private void HandleCamcorderState()
    {
        if (!isCameraUp) return;

        switch (currentCamMode)
        {
            case CamcorderMode.Idle:
                if (input.StartedRecording)
                    currentCamMode = CamcorderMode.Preparing;
                break;

            case CamcorderMode.Preparing:
                prepareTimer += Time.deltaTime;
                if (input.IsRecordingReleased)
                {
                    currentCamMode = CamcorderMode.Idle;
                    prepareTimer = 0f;
                }
                else if (prepareTimer >= prepareDuration)
                {
                    recorder.StartRecording();
                    currentCamMode = CamcorderMode.Recording;
                    GameEvents.PlayerModeChanged(PlayerMode.RecordingMode);
                    recordTimer = 0f;
                    prepareTimer = 0f;
                }
                break;

            case CamcorderMode.Recording:
                recordTimer += Time.deltaTime;

                // Tilt vertical: solo la cámara de la camcorder
                camcorderMotor.Tilt(input.RecordingTilt * mouseSensitivity);

                // Rotación horizontal: SOLO el body del player.
                // La camcorder sigue por ser hija — siempre mira al frente.
                float rotateDelta = input.RecordingRotate * mouseSensitivity
                                    * camcorderMotor.rotateSpeed * Time.deltaTime;
                playerMotor.RotateDirect(rotateDelta);

                if (input.IsRecordingReleased || recordTimer >= recordDuration)
                {
                    recorder.StopRecording();
                    storage.AddRecording(recorder.GetRecording());
                    recordTimer = 0f;
                    currentCamMode = CamcorderMode.Idle;
                    camcorderMotor.ResetRotation();
                    GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
                }
                break;
        }
    }
}