using UnityEngine;

/// <summary>
/// Los AudioSources en este GO solo son audibles durante grabación de camcorder.
/// Fuera de grabación, el volumen se zeroa (pero el audio sigue corriendo,
/// manteniendo el loop sincronizado).
/// </summary>
public class CamcorderOnlyAudio : MonoBehaviour
{
    private AudioSource[] sources;
    private float[] originalVolumes;

    private void Awake()
    {
        sources = GetComponents<AudioSource>();
        originalVolumes = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
            originalVolumes[i] = sources[i].volume;

        // Silenciamos desde el vamos — la fixed no debe oírlos nunca
        SetVolume(0f);
    }

    private void OnEnable()
    {
        GameEvents.OnRecordingStarted += OnRecordingStarted;
        GameEvents.OnRecordingStopped += OnRecordingStopped;
    }

    private void OnDisable()
    {
        GameEvents.OnRecordingStarted -= OnRecordingStarted;
        GameEvents.OnRecordingStopped -= OnRecordingStopped;
    }

    private void OnRecordingStarted() => RestoreVolume();
    private void OnRecordingStopped() => SetVolume(0f);

    private void SetVolume(float vol)
    {
        for (int i = 0; i < sources.Length; i++)
            sources[i].volume = vol;
    }

    private void RestoreVolume()
    {
        for (int i = 0; i < sources.Length; i++)
            sources[i].volume = originalVolumes[i];
    }
}