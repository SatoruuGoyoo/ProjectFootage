using UnityEngine;

[RequireComponent(typeof(CamcorderStorage))]
[RequireComponent(typeof(CamcorderInput))]
[RequireComponent(typeof(CamcorderMenuUI))]
[RequireComponent(typeof(CamcorderController))]
[RequireComponent(typeof(PlaybackClock))]
[RequireComponent(typeof(VideoPlayback))]
[RequireComponent(typeof(AudioPlayback))]
[RequireComponent(typeof(CamcorderMenuAudio))]

public class CamcorderMenuController : MonoBehaviour
{
    public static bool MenuInputBlocked = false;

    [Header("Menu UI")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private CamcorderMenuAnimator _menuAnimator;

    [Header("Views")]
    [SerializeField] private GameObject gridPanel;
    [SerializeField] private GameObject playbackPanel;

    [Header("Grid Navigation")]
    [SerializeField] private int columns = 2;

    [Header("Timing/TweakDesigner")]
    [SerializeField] private float rffStep = 1f;
    [SerializeField] private float rffDelay = 0.15f;


    private CamcorderStorage _storage;
    private CamcorderInput _input;
    private CamcorderMenuUI _ui;
    private CamcorderController _controller;
    private PlaybackClock _clock;
    private VideoPlayback _videoPlayback;
    private AudioPlayback _audioPlayback;
    private CamcorderMenuAudio _audio;


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
        _audio = GetComponent<CamcorderMenuAudio>();
    }

    private void OnEnable()
    {
        _clock.OnComplete += OnPlaybackComplete;
        _menuAnimator.OnCloseAnimationFinished += HandleCloseAnimationFinished;
    }

    private void OnDisable()
    {
        _clock.OnComplete -= OnPlaybackComplete;
        _menuAnimator.OnCloseAnimationFinished -= HandleCloseAnimationFinished;
    }

    private void Start()
    {
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
        menuCanvas.gameObject.SetActive(true);
        ShowGridView();
        _ui.UpdateUI(_currentIndex);
        _menuAnimator.PlayOpen();
    }

    private void CloseMenu()
    {
        StopEverything();

        IsMenuOpen = false;
        _currentIndex = 0;

        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        _menuAnimator.PlayClose();
    }

    private void ToggleMenu()
    {
        if (!_input.OpenCloseMenu) return;
        if (!IsMenuOpen && MenuInputBlocked) return;
        if (IsMenuOpen) CloseMenu();
        else OpenMenu();
    }

    private void ShowGridView()
    {
        if (gridPanel != null) gridPanel.SetActive(true);
        if (playbackPanel != null) playbackPanel.SetActive(false);
    }

    private void ShowPlaybackView()
    {
        if (gridPanel != null) gridPanel.SetActive(false);
        if (playbackPanel != null) playbackPanel.SetActive(true);
    }

    private void HandlePlayback()
    {
        if (_storage.Count == 0) return;
        if (!_input.PlayPauseRecording) return;

        _audio.PlayPlayPause();

        if (_clock.IsPlaying)
        {
            _clock.Pause();
        }
        else if (_clock.HasSession && !_clock.IsFinished)
        {
            _clock.Play();
        }
        else
        {
            RecordingSession session = _storage.GetRecording(_currentIndex);
            _clock.Load(session);
            _videoPlayback.Load(session);
            _audioPlayback.Load(session);
            _clock.Play();
            ShowPlaybackView();
        }
    }

    private void HandleRFF()
    {
        if (!_clock.HasSession || _clock.IsFinished) return;

        bool holdingRFF = _input.RewindRecording || _input.FastForwardRecording;

        if (!holdingRFF)
        {
            _rffTimer = 0f;
            _wasRFF = false;
            return;
        }

        if (!_wasRFF)
            _audio.PlayRFF();

        _wasRFF = true;

        _rffTimer += Time.deltaTime;
        if (_rffTimer < rffDelay) return;

        _rffTimer = 0f;

        if (_input.RewindRecording)
            _clock.SeekDelta(-rffStep);
        else
            _clock.SeekDelta(rffStep);
    }

    private void HandleStop()
    {
        if (!_input.StopRecording) return;
        if (!_clock.HasSession) return;

        _audio.PlayStopDiscard();

        StopEverything();
        ShowGridView();
        _ui.UpdateUI(_currentIndex);
    }

    private void HandleNavigation()
    {
        if (_storage.Count == 0) return;

        bool blockedByActiveSession = _clock.HasSession && !_clock.IsFinished;
        bool attemptedNavigate = _input.NavigateRight || _input.NavigateLeft
                              || _input.NavigateUp || _input.NavigateDown;

        if (blockedByActiveSession)
        {
            if (attemptedNavigate)
                _audio.PlayNavigateBlocked();
            return;
        }

        int previousIndex = _currentIndex;
        int count = _storage.Count;
        int cols = Mathf.Max(1, columns);

        if (_input.NavigateRight)
        {
            if (_currentIndex % cols < cols - 1 && _currentIndex + 1 < count)
                _currentIndex++;
        }
        else if (_input.NavigateLeft)
        {
            if (_currentIndex % cols > 0)
                _currentIndex--;
        }
        else if (_input.NavigateDown)
        {
            if (_currentIndex + cols < count)
                _currentIndex += cols;
        }
        else if (_input.NavigateUp)
        {
            if (_currentIndex - cols >= 0)
                _currentIndex -= cols;
        }

        _currentIndex = Mathf.Clamp(_currentIndex, 0, count - 1);

        if (_currentIndex != previousIndex)
            _audio.PlayNavigate();

        _ui.UpdateUI(_currentIndex);
    }

    private void HandleDiscard()
    {
        if (_storage.Count == 0) return;
        if (_clock.HasSession && !_clock.IsFinished) return;
        if (!_input.DiscardRecording) return;

        _audio.PlayStopDiscard();

        _storage.DiscardRecording(_currentIndex);
        _currentIndex = Mathf.Clamp(_currentIndex, 0, Mathf.Max(0, _storage.Count - 1));
        _ui.UpdateUI(_currentIndex);
    }

    private void OnPlaybackComplete()
    {
        ShowGridView();
        _ui.UpdateUI(_currentIndex);
    }

    private void StopEverything()
    {
        if (!_clock.HasSession) return;
        _clock.Stop();
    }

    private void HandleCloseAnimationFinished()
    {
        menuCanvas.gameObject.SetActive(false);
    }
}