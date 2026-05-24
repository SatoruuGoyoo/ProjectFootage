using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlaybackClock))]
public class VideoPlayback : MonoBehaviour
{
    [Header("Setup")]
    public RawImage displayImage;
    public GameObject playbackPanel;

    [Header("No Data")]
    public GameObject noDataOverlay;

    private PlaybackClock _clock;
    private RecordingSession _session;

    private Texture2D _displayTexture;

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

        displayImage.texture = _displayTexture;
    }

    private void OnPlay()
    {
        playbackPanel.SetActive(true);

        Debug.Log($"[VideoPlayback] play | session null? {_session == null} | corrupted? {_session?.IsCorrupted}");

        if (_session != null && _session.IsCorrupted)
        {
            ShowNoData();
            return;
        }

        if (noDataOverlay != null) noDataOverlay.SetActive(false);

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

    private void OnPause() { }

    private void OnStop()
    {
        playbackPanel.SetActive(false);
        if (noDataOverlay != null) noDataOverlay.SetActive(false);
        _session = null;
    }

    private void OnSeek(float time)
    {
        if (_session != null && _session.IsCorrupted) return;
        ShowFrameAtTime(time);
    }

    private void Update()
    {
        if (_session != null && _session.IsCorrupted) return;
        if (_clock.IsPlaying)
            ShowFrameAtTime(_clock.CurrentTime);
    }

    private void ShowNoData()
    {
        FillBlack();
        if (noDataOverlay != null) noDataOverlay.SetActive(true);
    }

    private void FillBlack()
    {
        if (_displayTexture == null) return;
        var pixels = new Color32[_displayTexture.width * _displayTexture.height];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(0, 0, 0, 255);
        _displayTexture.SetPixels32(pixels);
        _displayTexture.Apply();
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
    }
}