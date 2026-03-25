using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class CamcorderController : MonoBehaviour
{

    [Header("Setup")]
    public GameObject camcorderVisual;

    [Header("Timing/Runtime")]
    [SerializeField] private float prepareTimer = 0f;
    [SerializeField] private float recordTimer = 0f;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private int prepareDuration = 1; // Time required to prepare before recording
    [SerializeField] private int recordDuration = 5; // Minimum recording time to save

    // State variables
    private CamcorderMode currentCamMode = CamcorderMode.Idle;
    private PlayerMode currentPlayerMode = PlayerMode.ExplorationMode;
    private CamcorderRecorder recorder;
    private CamcorderPlayback playback;
    private CamcorderStorage storage;

    // Private methods and variables
    private CamcorderInput input;
    private bool isCameraUp = false;

    private void Awake()
    {
        input = GetComponent<CamcorderInput>();
        recorder = GetComponent<CamcorderRecorder>();
       // playback = GetComponent<CamcorderPlayback>();
        storage = GetComponent<CamcorderStorage>();
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

        HandleCamcorderState();

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

    private void ToggleCamera()
    {
        if (currentPlayerMode == PlayerMode.MenuCameraMode) return;

        isCameraUp = !isCameraUp;
        camcorderVisual.SetActive(isCameraUp);

        if (isCameraUp)
            GameEvents.PlayerModeChanged(PlayerMode.CameraMode);
        else
            GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

    private void HandleCamcorderState()
    {
        if (isCameraUp)
        {
            switch (currentCamMode)
            {
                case CamcorderMode.Idle:
                    if (input.StartedRecording)
                    {
                        currentCamMode = CamcorderMode.Preparing;
                        // Start preparing logic (e.g., show countdown UI)
                    }
                    break;

                case CamcorderMode.Preparing:
                    prepareTimer += Time.deltaTime;
                    Debug.Log("Preparing: " + prepareTimer);
                    if (input.IsRecordingReleased)
                    {
                        currentCamMode = CamcorderMode.Idle;
                        prepareTimer = 0f;
                        // Reset preparing logic (e.g., hide countdown UI)

                    }
                    else if (prepareTimer >= prepareDuration)
                    {
                        recorder.StartRecording();
                        currentCamMode = CamcorderMode.Recording;
                        recordTimer = 0f;
                        prepareTimer = 0f;
                        // Start recording logic (e.g., show recording UI)
                        // Handle preparing logic (e.g., countdown)
                    }
                    break;

                case CamcorderMode.Recording:
                    recordTimer += Time.deltaTime;
                    Debug.Log("Recording: " + recordTimer);
                    if (input.IsRecordingReleased || recordTimer >= recordDuration)
                    {
                        recorder.StopRecording();
                        storage.AddRecording(recorder.GetRecording());
                        recordTimer = 0f;
                        Debug.Log("Stop recording");
                        //playback.PlayRecording(recorder.GetRecording());
                        currentCamMode = CamcorderMode.Idle;
                    }
                    break;

            }
        }
    }

}