using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlaybackClock))]
public class VideoPlayback : MonoBehaviour
{
    [Header("Setup")]
    public RawImage displayImage;     
    public GameObject playbackPanel;  

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
        Debug.Log($"VideoPlayback sesión cargada {session.VideoFrames.Count} frames, duración {session.Duration}s");

        if (_displayTexture == null)
            _displayTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);

        displayImage.texture = _displayTexture;

    }

    private void OnPlay()
    {
        playbackPanel.SetActive(true);
        if (_session != null && _session.VideoFrames.Count > 0)
        {
            var firstFrame = _session.VideoFrames[0];
            Debug.Log($"Primer frame bytes {firstFrame.PixelData?.Length}, textura: {_displayTexture?.width}x{_displayTexture?.height}");

            if (firstFrame.PixelData != null && _displayTexture != null)
            {
                _displayTexture.LoadRawTextureData(firstFrame.PixelData);
                _displayTexture.Apply();
                Debug.Log($"displayImage.texture == _displayTexture: {displayImage.texture == _displayTexture}");
            }
        }
    }

    private void OnPause()
    {
        
    }

    private void OnStop()
    {
        playbackPanel.SetActive(false);
        _session = null;
    }

    private void OnSeek(float time)
    {
     
        ShowFrameAtTime(time);
    }

    private void Update()
    {

        if (_clock.IsPlaying)
            ShowFrameAtTime(_clock.CurrentTime);
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