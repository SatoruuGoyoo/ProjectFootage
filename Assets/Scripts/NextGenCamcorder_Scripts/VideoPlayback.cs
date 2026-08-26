using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using FMOD.Studio;
using FMODUnity;

[RequireComponent(typeof(PlaybackClock))]
public class VideoPlayback : MonoBehaviour
{
    [Header("Setup")]
    public RawImage displayImage;

    [Header("No Data")]
    public GameObject noDataOverlay;

    private PlaybackClock _clock;
    private RecordingSession _session;

    private Texture2D _displayTexture;

    private VideoPlayer _liveActionPlayer;
    private RenderTexture _liveActionTexture;
    private bool _playingLiveAction;

    private EventInstance _liveActionAudioInstance;
    private bool _hasLiveActionAudio;

    private void Awake()
    {
        _clock = GetComponent<PlaybackClock>();
    }

    private void OnEnable()
    {
        _clock.OnPlay += OnPlay;
        _clock.OnPause += OnPause;
        _clock.OnStop += OnStop;
        _clock.OnSeek += OnSeek;
        _clock.OnComplete += OnStop;
    }

    private void OnDisable()
    {
        _clock.OnPlay -= OnPlay;
        _clock.OnPause -= OnPause;
        _clock.OnStop -= OnStop;
        _clock.OnSeek -= OnSeek;
        _clock.OnComplete -= OnStop;
    }

    public void Load(RecordingSession session)
    {
        _session = session;

        if (_displayTexture == null)
            _displayTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);
    }

    private void OnPlay()
    {
        if (_session != null && _session.IsCorrupted)
        {
            ShowNoData();
            return;
        }

        if (displayImage != null) displayImage.gameObject.SetActive(true);
        if (noDataOverlay != null) noDataOverlay.SetActive(false);

        if (_session != null && _session.IsLiveAction)
        {
            StartLiveAction();
            return;
        }

        _playingLiveAction = false;
        displayImage.texture = _displayTexture;

        if (_session != null && _session.VideoFrames.Count > 0)
        {
            var firstFrame = _session.VideoFrames[0];
            if (firstFrame.PixelData != null && _displayTexture != null)
            {
                _displayTexture.LoadRawTextureData(firstFrame.PixelData);
                _displayTexture.Apply();
            }
        }
    }

    private void StartLiveAction()
    {
        _playingLiveAction = true;

        if (_liveActionPlayer == null)
        {
            _liveActionPlayer = gameObject.AddComponent<VideoPlayer>();
            _liveActionPlayer.playOnAwake = false;
            _liveActionPlayer.isLooping = false;
            _liveActionPlayer.renderMode = VideoRenderMode.RenderTexture;
            _liveActionPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        if (_liveActionTexture == null)
            _liveActionTexture = new RenderTexture(1920, 1080, 0);

        _liveActionPlayer.Stop();
        _liveActionPlayer.clip = _session.LiveActionClip;
        _liveActionPlayer.targetTexture = _liveActionTexture;
        displayImage.texture = _liveActionTexture;

        _liveActionPlayer.Play();

        StartLiveActionAudio();
    }

    private void StartLiveActionAudio()
    {
        _hasLiveActionAudio = false;
        if (_session == null || _session.LiveActionAudio.IsNull) return;

        _liveActionAudioInstance = RuntimeManager.CreateInstance(_session.LiveActionAudio);
        _liveActionAudioInstance.start();
        _hasLiveActionAudio = true;
    }

    private void StopLiveActionAudio()
    {
        if (!_hasLiveActionAudio) return;
        _liveActionAudioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _liveActionAudioInstance.release();
        _hasLiveActionAudio = false;
    }

    private void OnPause()
    {
        if (_playingLiveAction && _liveActionPlayer != null)
            _liveActionPlayer.Pause();

        if (_hasLiveActionAudio)
            _liveActionAudioInstance.setPaused(true);
    }

    private void OnStop()
    {
        if (noDataOverlay != null) noDataOverlay.SetActive(false);
        if (displayImage != null) displayImage.gameObject.SetActive(true);

        if (_liveActionPlayer != null) _liveActionPlayer.Stop();
        StopLiveActionAudio();
        _playingLiveAction = false;
        _session = null;
    }

    private void OnSeek(float time)
    {
        if (_session != null && _session.IsCorrupted) return;

        if (_playingLiveAction)
        {
            if (_liveActionPlayer != null && _liveActionPlayer.canSetTime)
                _liveActionPlayer.time = time;

            if (_hasLiveActionAudio)
                _liveActionAudioInstance.setTimelinePosition(Mathf.RoundToInt(time * 1000f));
            return;
        }

        ShowFrameAtTime(time);
    }

    private void Update()
    {
        if (_session != null && _session.IsCorrupted) return;
        if (_playingLiveAction) return;
        if (_clock.IsPlaying)
            ShowFrameAtTime(_clock.CurrentTime);
    }

    private void ShowNoData()
    {
        if (displayImage != null) displayImage.gameObject.SetActive(false);
        if (noDataOverlay != null) noDataOverlay.SetActive(true);
    }

    private void ShowFrameAtTime(float time)
    {
        if (_session == null) return;

        VideoFrame? frame = _session.GetFrameAtTime(time);
        if (frame == null) return;

        _displayTexture.LoadRawTextureData(frame.Value.PixelData);
        _displayTexture.Apply();
    }

    private void OnDestroy()
    {
        if (_displayTexture != null)
            Destroy(_displayTexture);
        if (_liveActionTexture != null)
        {
            _liveActionTexture.Release();
            Destroy(_liveActionTexture);
        }
        StopLiveActionAudio();
    }
}