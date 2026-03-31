using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderPlayback : MonoBehaviour
{
    [Header("Setup")]
    public GameObject playbackPanel;
    public RawImage playbackImage;

    public bool HasRecording => framesToPlay != null && framesToPlay.Count > 0;
    public bool IsFinished => framesToPlay != null && currentFrame >= framesToPlay.Count;

    private List<Texture2D> framesToPlay;
    private int currentFrame = 0;
    private float playbackTimer = 0f;
    [SerializeField] private float playbackInterval = 0.125f;

    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        playbackPanel.SetActive(false);
    }

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

    public void PlayRecording(List<Texture2D> frames)
    {
        framesToPlay = frames;
        currentFrame = 0;
        playbackTimer = 0f;
        isPlaying = true;
        playbackPanel.SetActive(true);
    }

    public void PausePlayback()
    {
        isPlaying = false;
    }

    public void ResumePlayback()
    {
        isPlaying = true;
    }

    private void ShowNextFrame()
    {
        if (currentFrame >= framesToPlay.Count)
        {
            isPlaying = false;
            playbackPanel.SetActive(false);
            return;
        }

        playbackImage.texture = framesToPlay[currentFrame];
        currentFrame++;
    }

    public void RewindFrame()
    {
        if (currentFrame > 0)
        {
            GameEvents.FrameChanged(currentFrame - 1);
            currentFrame--;
            playbackImage.texture = framesToPlay[currentFrame];

            // Maybe Clamp?
        }
    }

    public void FastForwardFrame()
    {
        if (currentFrame < framesToPlay.Count - 1)
        {
            GameEvents.FrameChanged(currentFrame + 1);
            currentFrame++;
            playbackImage.texture = framesToPlay[currentFrame];

            // Maybe Clamp?
        }
    }
}