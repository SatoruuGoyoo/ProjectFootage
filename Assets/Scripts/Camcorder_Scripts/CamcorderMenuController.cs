using UnityEngine;

public class CamcorderMenuController : MonoBehaviour
{
    [Header("FPS View")]
    public Camera fpsCamera;
    public GameObject fpsHandModel;

    [Header("Menu UI")]
    public Canvas menuCanvas;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private float rffStep = 1f; // Seconds to seek on each RFF step
    [SerializeField] private float rffDelay = 0.15f; // Rewind delay

    private CamcorderStorage _storage;
    private CamcorderInput _input;
    private CamcorderMenuUI _ui;
    private CamcorderController _controller;
    private PlaybackClock _clock;
    private VideoPlayback _videoPlayback;
    private AudioPlayback _audioPlayback;
    //private IterationPlaybackAudio[] _iterationAudios;

    private Camera _activeFixedCamera;
    private int _currentIndex = 0;
    private float _rffTimer = 0f;
    private bool _wasRFF = false;

    public bool IsMenuOpen { get; private set; }

    private void Awake()
    {
        _storage = GetComponent<CamcorderStorage>();
        _input = GetComponent<CamcorderInput>();
        _ui = GetComponent<CamcorderMenuUI>();
        _controller = GetComponent<CamcorderController>();
        _clock = GetComponent<PlaybackClock>();
        _videoPlayback = GetComponent<VideoPlayback>();
        _audioPlayback = GetComponent<AudioPlayback>();
        //_iterationAudios = FindObjectsByType<IterationPlaybackAudio>(FindObjectsSortMode.None);
    }

    private void OnEnable()
    {
        _clock.OnComplete += OnPlaybackComplete;
    }

    private void OnDisable()
    {
        _clock.OnComplete -= OnPlaybackComplete;
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
        if (!IsMenuOpen) return;

        HandleNavigation();
        HandlePlayback();
        HandleRFF();
        HandleStop();
        HandleDiscard();
    }
    private void OpenMenu()
    {
        if (_controller.CurrentCamMode == CamcorderMode.Recording) return;

        IsMenuOpen = true;
        GameEvents.PlayerModeChanged(PlayerMode.MenuCameraMode);

        _activeFixedCamera = FindActiveFixedCamera();
        if (_activeFixedCamera != null)
            _activeFixedCamera.gameObject.SetActive(false);

        fpsCamera.gameObject.SetActive(true);
        fpsHandModel.SetActive(true);
        menuCanvas.gameObject.SetActive(true);
        _ui.UpdateUI(_currentIndex);
    }

    private void CloseMenu()
    {
        StopEverything();

        IsMenuOpen = false;
        _currentIndex = 0;

        fpsCamera.gameObject.SetActive(false);
        fpsHandModel.SetActive(false);
        menuCanvas.gameObject.SetActive(false);

        if (_activeFixedCamera != null)
            _activeFixedCamera.gameObject.SetActive(true);
        _activeFixedCamera = null;

        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

    private void ToggleMenu()
    {
        if (!_input.OpenCloseMenu) return;
        if (IsMenuOpen) CloseMenu();
        else OpenMenu();
    }
    private void HandlePlayback()
    {
        if (_storage.Count == 0) return;
        if (!_input.PlayPauseRecording) return;

        if (_clock.IsPlaying)
        {
            // Pause
            _clock.Pause();
           // PlaybackAudioManager.Instance?.OnPlaybackPaused();
            //foreach (var a in _iterationAudios) a.OnPlaybackPaused();
        }
        else if (_clock.HasSession && !_clock.IsFinished)
        {
            // Resume
            _clock.Play();
           // PlaybackAudioManager.Instance?.OnPlaybackStarted(isResume: true);
           // foreach (var a in _iterationAudios) a.OnPlaybackStarted(isResume: true);
        }
        else
        {
            RecordingSession session = _storage.GetRecording(_currentIndex);

            _clock.Load(session);

            _videoPlayback.Load(session);
            _audioPlayback.Load(session);

            _clock.Play();
           // PlaybackAudioManager.Instance?.OnPlaybackStarted(isResume: false);
           // foreach (var a in _iterationAudios) a.OnPlaybackStarted(isResume: false);
        }
    }

    private void HandleRFF()
    {
        if (!_clock.HasSession || _clock.IsFinished) return;
        if (!_input.RewindRecording && !_input.FastForwardRecording)
        {
            _rffTimer = 0f;
            if (_wasRFF)
            {
                _wasRFF = false;
               // PlaybackAudioManager.Instance?.OnRFFStopped();
            }
            return;
        }

        _rffTimer += Time.deltaTime;
        if (_rffTimer < rffDelay) return;

        _rffTimer = 0f;
        _wasRFF = true;

        if (_input.RewindRecording)
        {
            _clock.SeekDelta(-rffStep);
           // PlaybackAudioManager.Instance?.OnRFF(isRewind: true);
        }
        else
        {
            _clock.SeekDelta(rffStep);
            // PlaybackAudioManager.Instance?.OnRFF(isRewind: false);
        }
    }

    private void HandleStop()
    {
        if (!_input.StopRecording) return;
        if (!_clock.HasSession) return;

        StopEverything();
        _ui.UpdateUI(_currentIndex);
    }

    private void HandleNavigation()
    {
        if (_storage.Count == 0) return;

        if (_input.NavigateRight) _currentIndex++;
        else if (_input.NavigateLeft) _currentIndex--;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, _storage.Count - 1);
        _ui.UpdateUI(_currentIndex);
    }

    private void HandleDiscard()
    {
        if (_storage.Count == 0) return;
        if (_clock.IsPlaying) return;
        if (!_input.DiscardRecording) return;

        _storage.DiscardRecording(_currentIndex);
        _currentIndex = Mathf.Clamp(_currentIndex, 0, Mathf.Max(0, _storage.Count - 1));
        _ui.UpdateUI(_currentIndex);
    }

    private void OnPlaybackComplete()
    {
       // PlaybackAudioManager.Instance?.OnPlaybackStopped();
        //foreach (var a in _iterationAudios) a.OnPlaybackStopped();
    }

    private void StopEverything()
    {
        if (!_clock.HasSession) return;
        _clock.Stop();
       // PlaybackAudioManager.Instance?.OnPlaybackStopped();
        //foreach (var a in _iterationAudios) a.OnPlaybackStopped();
    }

    private Camera FindActiveFixedCamera()
    {
        foreach (Camera cam in Camera.allCameras)
            if (cam != fpsCamera && cam.enabled) return cam;
        return null;
    }
}