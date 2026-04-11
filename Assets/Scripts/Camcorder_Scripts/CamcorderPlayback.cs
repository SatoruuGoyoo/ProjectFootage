using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderPlayback : MonoBehaviour
{
    [Header("Setup")]
    public GameObject playbackPanel;
    public RawImage playbackImage;
    public AudioSource audioSource;

    public bool HasRecording => framesToPlay != null && framesToPlay.Count > 0;
    public bool IsFinished => framesToPlay != null && currentFrame >= framesToPlay.Count;
    public bool IsPlaying => isPlaying;
    public bool PlayerUsedRFF { get; private set; } = false;
    public int CurrentFrame => currentFrame;

    private List<Texture2D> framesToPlay;
    private int currentFrame = 0;
    private float playbackTimer = 0f;
    [SerializeField] private float playbackInterval = 0.125f;
    private bool isPlaying = false;

    private void Start() => playbackPanel.SetActive(false);

    private void Update()
    {
        if (!isPlaying) return;
        playbackTimer += Time.deltaTime;
        if (playbackTimer > playbackInterval)
        {
            playbackTimer = 0f;
            ShowNextFrame();
        }
    }

    public void PlayRecording(RecordingData data)
    {
        framesToPlay = data.frames;
        currentFrame = 0;
        playbackTimer = 0f;
        isPlaying = true;
        PlayerUsedRFF = false;
        playbackPanel.SetActive(true);

        if (data.clip != null && audioSource != null)
        {
            audioSource.clip = data.clip;
            audioSource.Play();
        }
    }

    public void PausePlayback()
    {
        isPlaying = false;
        audioSource?.Pause();
    }

    public void ResumePlayback()
    {
        isPlaying = true;
        audioSource?.UnPause();
    }

    public void RewindFrame()
    {
        PlayerUsedRFF = true;
        if (currentFrame > 0)
        {
            currentFrame--;
            playbackImage.texture = framesToPlay[currentFrame];
            GameEvents.FrameChanged(currentFrame);
            SyncAudio();
        }
    }

    public void FastForwardFrame()
    {
        PlayerUsedRFF = true;
        if (currentFrame < framesToPlay.Count - 1)
        {
            currentFrame++;
            playbackImage.texture = framesToPlay[currentFrame];
            GameEvents.FrameChanged(currentFrame);
            SyncAudio();
        }
    }

    private void ShowNextFrame()
    {
        if (currentFrame >= framesToPlay.Count)
        {
            isPlaying = false;
            playbackPanel.SetActive(false);
            audioSource?.Stop();
            return;
        }
        playbackImage.texture = framesToPlay[currentFrame];
        currentFrame++;
    }

    private void SyncAudio()
    {
        if (audioSource == null || audioSource.clip == null) return;
        audioSource.time = ((float)currentFrame / framesToPlay.Count) * audioSource.clip.length;
    }
}