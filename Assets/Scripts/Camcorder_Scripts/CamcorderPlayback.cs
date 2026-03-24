using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CamcorderPlayback : MonoBehaviour
{
    public RawImage debugImage;
    public List<Texture2D> framesToPlay;
    private int currentFrame = 0;
    private float playbackTimer = 0f;
    [SerializeField] private float playbackInterval = 0.125f;
    private bool isPlaying = false;

    private void Update()
    {
        if (!isPlaying) return;

        playbackTimer += Time.deltaTime;
        if(playbackTimer > playbackInterval)
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
        debugImage.gameObject.SetActive(true); // activás al arrancar
    }

    private void ShowNextFrame()
    {
        if (currentFrame >= framesToPlay.Count)
        {
            isPlaying = false;
            debugImage.gameObject.SetActive(false); // desactivás al terminar
            return;
        }

        debugImage.texture = framesToPlay[currentFrame];
        currentFrame++;
    }



}
