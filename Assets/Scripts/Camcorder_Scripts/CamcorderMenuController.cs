using UnityEngine;

public class CamcorderMenuController : MonoBehaviour
{
    [Header("FPS View")]
    public Camera fpsCamera;
    public GameObject fpsHandModel;

    [Header("Menu UI")]
    public Canvas menuCanvas;

    //[Header("Transition")]
    //public CamcorderTransition transition;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private float frameStepDelay = 0.15f;

    private CamcorderStorage storage;
    private CamcorderPlayback playback;
    private CamcorderInput input;
    private CamcorderMenuUI ui;
    private CamcorderController controller;

    private Camera activeFixedCamera;
    private float frameStepTimer = 0f;
    private int currentRecordingIndex = 0;

    private bool isMenuOpen = false;
    public bool IsMenuOpen => isMenuOpen;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
        playback = GetComponent<CamcorderPlayback>();
        input = GetComponent<CamcorderInput>();
        ui = GetComponent<CamcorderMenuUI>();
        controller = GetComponent<CamcorderController>();
    }

    private void Start()
    {
        fpsCamera.gameObject.SetActive(false);
        fpsHandModel.SetActive(false);
        menuCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        ToggleMenu();

        if (isMenuOpen)
        {
            HandleNavigation();
            HandlePlayback();
            HandleDiscard();
        }
    }

    private void OpenMenu()
    {
        //if (isMenuOpen || transition.IsTransitioning) return;
        if (controller.CurrentCamMode == CamcorderMode.Recording) return;

        isMenuOpen = true;
        GameEvents.PlayerModeChanged(PlayerMode.MenuCameraMode);
        activeFixedCamera = FindActiveFixedCamera();

        
            if (activeFixedCamera != null)
                activeFixedCamera.gameObject.SetActive(false);

            fpsCamera.gameObject.SetActive(true);
            fpsHandModel.SetActive(true);
            menuCanvas.gameObject.SetActive(true);
            ui.UpdateUI(currentRecordingIndex);
        
    }

    private void CloseMenu()
    {
        

        isMenuOpen = false;
        currentRecordingIndex = 0;
        fpsCamera.gameObject.SetActive(false);
        fpsHandModel.SetActive(false);
        menuCanvas.gameObject.SetActive(false);

        if (activeFixedCamera != null)
            activeFixedCamera.gameObject.SetActive(true);

        activeFixedCamera = null;
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);

        
    }

    private void ToggleMenu()
    {
        if (!input.OpenCloseMenu) return;

        if (isMenuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    private void HandleNavigation()
    {
        if (storage.GetAllRecordings().Count == 0) return;

        if (input.NavigateRight)
            currentRecordingIndex++;
        else if (input.NavigateLeft)
            currentRecordingIndex--;

        currentRecordingIndex = Mathf.Clamp(currentRecordingIndex, 0, storage.GetAllRecordings().Count - 1);
        ui.UpdateUI(currentRecordingIndex);
    }

    private void HandlePlayback()
    {
        if (storage.GetAllRecordings().Count == 0) return;

        if (input.PlayPauseRecording)
        {
            if (playback.IsPlaying)
                playback.PausePlayback();
            else if (playback.HasRecording && !playback.IsFinished)
                playback.ResumePlayback();
            else
                playback.PlayRecording(storage.GetAllRecordings()[currentRecordingIndex]);
        }

        if (!playback.IsPlaying && playback.HasRecording)
        {
            if (input.RewindRecording || input.FastForwardRecording)
            {
                frameStepTimer += Time.deltaTime;
                if (frameStepTimer >= frameStepDelay)
                {
                    frameStepTimer = 0f;
                    if (input.RewindRecording) playback.RewindFrame();
                    if (input.FastForwardRecording) playback.FastForwardFrame();
                }
            }
            else
            {
                frameStepTimer = 0f;
            }
        }
    }

    private void HandleDiscard()
    {
        if (storage.GetAllRecordings().Count == 0) return;
        if (playback.IsPlaying) return;
        if (!input.DiscardRecording) return;

        storage.DiscardRecording(currentRecordingIndex);

        if (storage.GetAllRecordings().Count == 0)
            currentRecordingIndex = 0;
        else
            currentRecordingIndex = Mathf.Clamp(currentRecordingIndex, 0, storage.GetAllRecordings().Count - 1);

        ui.UpdateUI(currentRecordingIndex);
    }

    private Camera FindActiveFixedCamera()
    {
        foreach (Camera cam in Camera.allCameras)
        {
            if (cam != fpsCamera && cam.enabled)
                return cam;
        }
        return null;
    }
}