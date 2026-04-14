using UnityEngine;

/// <summary>
/// Captura el audio que pasa por el AudioListener durante la grabación de la camcorder.
/// Colocá este componente en el mismo GameObject que tiene el AudioListener activo.
///
/// Uso:
///   audioCapture.StartCapture();   // al iniciar grabación
///   audioCapture.StopCapture();    // al detener grabación
///   AudioClip clip = audioCapture.GetCapturedClip();  // obtener el audio grabado
///
/// Técnico: OnAudioFilterRead corre en el audio thread.
/// Se usa un buffer preallocado con writeIndex volatile para thread safety.
/// El audio pasa sin modificaciones (pass-through) — el jugador sigue escuchando normal.
/// </summary>
[RequireComponent(typeof(AudioListener))]
public class CamcorderAudioCapture : MonoBehaviour
{
    [Tooltip("Duración máxima de captura en segundos. Define el tamaño del buffer preallocado.")]
    [SerializeField] private float maxCaptureDuration = 10f;

    private float[] buffer;
    private volatile int writeIndex;
    private volatile bool isCapturing;
    private int capturedSampleRate;

    private void Awake()
    {
        capturedSampleRate = AudioSettings.outputSampleRate;
        int bufferSize = capturedSampleRate * Mathf.CeilToInt(maxCaptureDuration);
        buffer = new float[bufferSize];
    }

    /// <summary>Inicia la captura de audio. Limpia el buffer previo.</summary>
    public void StartCapture()
    {
        writeIndex = 0;
        capturedSampleRate = AudioSettings.outputSampleRate;
        isCapturing = true;
    }

    /// <summary>Detiene la captura de audio.</summary>
    public void StopCapture()
    {
        isCapturing = false;
    }

    /// <summary>
    /// Devuelve un AudioClip mono con el audio capturado desde el último StartCapture.
    /// Retorna null si no se capturó nada.
    /// </summary>
    public AudioClip GetCapturedClip()
    {
        int samples = writeIndex;
        if (samples == 0) return null;

        float[] trimmed = new float[samples];
        System.Array.Copy(buffer, trimmed, samples);

        AudioClip clip = AudioClip.Create("CamcorderAudio", samples, 1, capturedSampleRate, false);
        clip.SetData(trimmed, 0);
        return clip;
    }

    /// <summary>
    /// Callback del audio thread. Captura samples sin modificar la señal (pass-through).
    /// Convierte a mono promediando canales.
    /// </summary>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isCapturing) return;

        for (int i = 0; i < data.Length; i += channels)
        {
            int idx = writeIndex;
            if (idx >= buffer.Length) return;

            float mono = 0f;
            for (int ch = 0; ch < channels; ch++)
                mono += data[i + ch];

            buffer[idx] = mono / channels;
            writeIndex = idx + 1;
        }
    }
}
