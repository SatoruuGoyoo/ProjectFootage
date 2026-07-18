// CamcorderMenuController.cs
// Cambio principal: ya no manejamos _activeFixedCamera para esconder/mostrar.
// En overlay el canvas tapa todo visualmente solo.

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
    }

    private void OnDisable()
    {
        _clock.OnComplete -= OnPlaybackComplete;
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
        _ui.UpdateUI(_currentIndex);
    }

    private void CloseMenu()
    {
        StopEverything();

        IsMenuOpen = false;
        _currentIndex = 0;
        menuCanvas.gameObject.SetActive(false);

        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
    }

    private void ToggleMenu()
    {
        if (!_input.OpenCloseMenu) return;
        if (!IsMenuOpen && MenuInputBlocked) return;
        if (IsMenuOpen) CloseMenu();
        else OpenMenu();
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
        _ui.UpdateUI(_currentIndex);
    }

    private void HandleNavigation()
    {
        if (_storage.Count == 0) return;

        bool blockedByActiveSession = _clock.HasSession && !_clock.IsFinished;
        bool attemptedNavigate = _input.NavigateRight || _input.NavigateLeft;

        if (blockedByActiveSession)
        {
            if (attemptedNavigate)
                _audio.PlayNavigateBlocked();
            return;
        }

        int previousIndex = _currentIndex;

        if (_input.NavigateRight) _currentIndex++;
        else if (_input.NavigateLeft) _currentIndex--;

        _currentIndex = Mathf.Clamp(_currentIndex, 0, _storage.Count - 1);

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
    }

    private void StopEverything()
    {
        if (!_clock.HasSession) return;
        _clock.Stop();
    }
}