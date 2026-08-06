using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class RenderVideoStep : SequenceStep
{
    [Header("Video")]
    [SerializeField] private VideoClip clip;
    [SerializeField] private RawImage videoDisplay;

    [Header("Skip")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private float skipHoldDuration = 0.5f;

    [Header("FMOD")]
    [SerializeField] private string audioEvent = "event:/MainMenu/Ambient/IntroVideo";

    [Header("Flow")]
    [SerializeField] private bool waitForCompletion = true;

    private VideoPlayer _videoPlayer;
    private RenderTexture _renderTexture;

    protected override void OnExecute()
    {
        StartCoroutine(RunVideo());
    }

    private IEnumerator RunVideo()
    {
        if (clip == null || videoDisplay == null)
        {
            Complete();
            yield break;
        }

        SetupPlayer();

        _videoPlayer.clip = clip;
        _videoPlayer.EnableAudioTrack(0, false);
        _videoPlayer.Prepare();

        while (!_videoPlayer.isPrepared)
            yield return null;

        FMOD.Studio.EventInstance audioInstance = default;
        bool hasAudio = !string.IsNullOrEmpty(audioEvent);
        if (hasAudio)
        {
            audioInstance = FMODUnity.RuntimeManager.CreateInstance(audioEvent);
            audioInstance.start();
        }

        videoDisplay.gameObject.SetActive(true);
        _videoPlayer.Play();

        if (!waitForCompletion)
            Complete();

        float skipTimer = 0f;
        while (_videoPlayer.isPlaying)
        {
            if (allowSkip && Input.GetKey(skipKey))
            {
                skipTimer += Time.unscaledDeltaTime;
                if (skipTimer >= skipHoldDuration)
                    break;
            }
            else
            {
                skipTimer = 0f;
            }
            yield return null;
        }

        if (hasAudio)
        {
            audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            audioInstance.release();
        }

        videoDisplay.gameObject.SetActive(false);
        _videoPlayer.Stop();

        Cleanup();

        if (waitForCompletion)
            Complete();
    }

    private void SetupPlayer()
    {
        _videoPlayer = videoDisplay.gameObject.GetComponent<VideoPlayer>();
        if (_videoPlayer == null)
            _videoPlayer = videoDisplay.gameObject.AddComponent<VideoPlayer>();

        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.isLooping = false;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        _renderTexture = new RenderTexture(1920, 1080, 0);
        _videoPlayer.targetTexture = _renderTexture;
        videoDisplay.texture = _renderTexture;
    }

    private void Cleanup()
    {
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            Destroy(_renderTexture);
            _renderTexture = null;
        }
    }
}