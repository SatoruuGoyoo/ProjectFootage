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
    public bool PlayerUsedRFF { get; private set; } = false;

    private List<Texture2D> framesToPlay;
    private int currentFrame = 0;
    public int CurrentFrame => currentFrame;
    private float playbackTimer = 0f;
    [SerializeField] private float playbackInterval = 0.125f;

    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    // ── Diegetic audio ─────────────────────────────────────────────────────
    // audioSamplesPerSource[sourceIndex][frameIndex] → volumen 0-1
    private List<List<float>> audioSamplesPerSource;
    private CamcorderDiegeticAudio[] diegeticSources;

    // ── Unity ──────────────────────────────────────────────────────────────
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

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Inicia la reproducción. Opcionalmente recibe los samples de audio
    /// grabados y las referencias a los AudioSources diegéticos para modularlos.
    /// </summary>
    public void PlayRecording(List<Texture2D> frames,
                              List<List<float>> audioSamples = null,
                              CamcorderDiegeticAudio[] audioSources = null)
    {
        framesToPlay = frames;
        audioSamplesPerSource = audioSamples;
        diegeticSources = audioSources;

        currentFrame = 0;
        playbackTimer = 0f;
        isPlaying = true;
        playbackPanel.SetActive(true);
        PlayerUsedRFF = false;

        ApplyAudioFrame(0);
    }

    public void PausePlayback()
    {
        isPlaying = false;
    }

    public void ResumePlayback()
    {
        isPlaying = true;
    }

    public void RewindFrame()
    {
        PlayerUsedRFF = true;
        if (currentFrame > 0)
        {
            GameEvents.FrameChanged(currentFrame - 1);
            currentFrame--;
            playbackImage.texture = framesToPlay[currentFrame];
            ApplyAudioFrame(currentFrame);
        }
    }

    public void FastForwardFrame()
    {
        PlayerUsedRFF = true;
        if (currentFrame < framesToPlay.Count - 1)
        {
            GameEvents.FrameChanged(currentFrame + 1);
            currentFrame++;
            playbackImage.texture = framesToPlay[currentFrame];
            ApplyAudioFrame(currentFrame);
        }
    }

    // ── Privado ────────────────────────────────────────────────────────────

    private void ShowNextFrame()
    {
        if (currentFrame >= framesToPlay.Count)
        {
            isPlaying = false;
            playbackPanel.SetActive(false);
            ResetAllAudioSources();
            return;
        }

        playbackImage.texture = framesToPlay[currentFrame];
        ApplyAudioFrame(currentFrame);
        currentFrame++;
    }

    /// <summary>
    /// Aplica el volumen grabado de cada fuente diegética para el frame dado.
    /// </summary>
    private void ApplyAudioFrame(int frameIndex)
    {
        if (diegeticSources == null || audioSamplesPerSource == null) return;

        for (int i = 0; i < diegeticSources.Length; i++)
        {
            if (diegeticSources[i] == null) continue;

            // Si hay samples grabados para esta fuente y este frame, los usamos
            if (i < audioSamplesPerSource.Count &&
                frameIndex < audioSamplesPerSource[i].Count)
            {
                diegeticSources[i].ApplyPlaybackVolume(audioSamplesPerSource[i][frameIndex]);
            }
            else
            {
                // Frame sin metadato → silencio
                diegeticSources[i].ApplyPlaybackVolume(0f);
            }
        }
    }

    private void ResetAllAudioSources()
    {
        if (diegeticSources == null) return;

        foreach (var source in diegeticSources)
            source?.ResetVolume();
    }
}