using UnityEngine;

public class CamcorderMenuController : MonoBehaviour
{
    [Header("FPS View")]
    public Camera fpsCamera;
    public GameObject fpsHandModel;

    [Header("Menu UI")]
    public Canvas menuCanvas;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private float frameStepDelay = 0.15f;

    private CamcorderStorage storage;
    private CamcorderPlayback playback;
    private CamcorderInput input;
    private CamcorderMenuUI ui;
    private CamcorderController controller;
    private IterationPlaybackAudio[] iterationAudios;

    private Camera activeFixedCamera;
    private float frameStepTimer = 0f;
    private int currentRecordingIndex = 0;
    private bool wasRFF = false;

    private bool isMenuOpen = false;
    public bool IsMenuOpen => isMenuOpen;

    private void OnEnable() => GameEvents.OnPlaybackEnded += OnPlaybackEnded;
    private void OnDisable() => GameEvents.OnPlaybackEnded -= OnPlaybackEnded;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
        playback = GetComponent<CamcorderPlayback>();
        input = GetComponent<CamcorderInput>();
        ui = GetComponent<CamcorderMenuUI>();
        controller = GetComponent<CamcorderController>();
        iterationAudios = FindObjectsByType<IterationPlaybackAudio>(FindObjectsSortMode.None);
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
            HandleStop();
            HandleDiscard();
        }
    }

    private void OpenMenu()
    {
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
        // Fix: llaves correctas para que el foreach esté dentro del if
        if (playback.HasRecording)
        {
            playback.StopPlayback();
            PlaybackAudioManager.Instance?.OnPlaybackStopped();
            foreach (var audio in iterationAudios) audio.OnPlaybackStopped();
        }

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
        if (isMenuOpen) CloseMenu();
        else OpenMenu();
    }

    private void OnPlaybackEnded()
    {
        PlaybackAudioManager.Instance?.OnPlaybackStopped();
        foreach (var audio in iterationAudios) audio.OnPlaybackStopped();
    }

    private void HandleNavigation()
    {
        if (storage.GetAllRecordings().Count == 0) return;

        if (input.NavigateRight) currentRecordingIndex++;
        else if (input.NavigateLeft) currentRecordingIndex--;

        currentRecordingIndex = Mathf.Clamp(currentRecordingIndex, 0, storage.GetAllRecordings().Count - 1);
        ui.UpdateUI(currentRecordingIndex);
    }

    private void HandlePlayback()
    {
        if (storage.GetAllRecordings().Count == 0) return;

        if (input.PlayPauseRecording)
        {
            if (playback.IsPlaying)
            {
                playback.PausePlayback();
                PlaybackAudioManager.Instance?.OnPlaybackPaused();
                foreach (var audio in iterationAudios) audio.OnPlaybackPaused();
            }
            else if (playback.HasRecording && !playback.IsFinished)
            {
                playback.ResumePlayback();
                PlaybackAudioManager.Instance?.OnPlaybackStarted(true);
                foreach (var audio in iterationAudios) audio.OnPlaybackStarted(true);
            }
            else
            {
                playback.PlayRecording(storage.GetAllRecordings()[currentRecordingIndex]);
                PlaybackAudioManager.Instance?.OnPlaybackStarted(false);
                foreach (var audio in iterationAudios) audio.OnPlaybackStarted(false);
            }
        }

        if (playback.HasRecording && !playback.IsFinished)
        {
            if (input.RewindRecording || input.FastForwardRecording)
            {
                frameStepTimer += Time.deltaTime;
                if (frameStepTimer >= frameStepDelay)
                {
                    frameStepTimer = 0f;
                    wasRFF = true;
                    if (input.RewindRecording)
                    {
                        playback.RewindFrame();
                        PlaybackAudioManager.Instance?.OnRFF(true);
                    }
                    if (input.FastForwardRecording)
                    {
                        playback.FastForwardFrame();
                        PlaybackAudioManager.Instance?.OnRFF(false);
                    }
                }
            }
            else
            {
                frameStepTimer = 0f;
                if (wasRFF)
                {
                    wasRFF = false;
                    PlaybackAudioManager.Instance?.OnRFFStopped();
                }
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

    private void HandleStop()
    {
        if (!input.StopRecording) return;
        if (!playback.HasRecording) return;

        playback.StopPlayback();
        PlaybackAudioManager.Instance?.OnPlaybackStopped();
        foreach (var audio in iterationAudios) audio.OnPlaybackStopped();
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