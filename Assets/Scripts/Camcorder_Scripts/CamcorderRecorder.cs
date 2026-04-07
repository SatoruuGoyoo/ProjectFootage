using System.Collections.Generic;
using UnityEngine;

public class CamcorderRecorder : MonoBehaviour
{
    [Header("Recording")]
    public RenderTexture recordingTexture;
    public List<Texture2D> framesRecorded = new List<Texture2D>();
    public List<Texture2D> GetRecording() => new List<Texture2D>(framesRecorded);

    [Header("Diegetic Audio")]
    [Tooltip("Transform del punto de vista de la cámara (el mismo que apunta hacia adelante).")]
    public Transform camViewpoint;

    [Tooltip("Todas las fuentes de audio diegéticas de la escena que esta cámara puede capturar.")]
    public CamcorderDiegeticAudio[] diegeticAudioSources;

    // ── Audio samples por fuente: audioSamples[sourceIndex][frameIndex] ──
    private List<List<float>> audioSamples = new List<List<float>>();

    public bool IsRecording { get; private set; } = false;

    private float captureTimer = 0f;
    [SerializeField] private float captureInterval = 0.125f;

    // ── Unity ──────────────────────────────────────────────────────────────
    private void Update()
    {
        if (!IsRecording) return;

        captureTimer += Time.deltaTime;
        if (captureTimer >= captureInterval)
        {
            captureTimer = 0f;
            CaptureFrame();
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void StartRecording()
    {
        framesRecorded.Clear();
        audioSamples.Clear();

        // Inicializamos una lista de samples por cada fuente registrada
        if (diegeticAudioSources != null)
        {
            foreach (var _ in diegeticAudioSources)
                audioSamples.Add(new List<float>());
        }

        captureTimer = 0f;
        IsRecording = true;
    }

    public void StopRecording()
    {
        IsRecording = false;
        captureTimer = 0f;
    }

    /// <summary>
    /// Devuelve los samples de audio grabados para una fuente específica.
    /// Usalo en CamcorderPlayback para modular el volumen frame a frame.
    /// </summary>
    public List<float> GetAudioSamples(int sourceIndex)
    {
        if (audioSamples == null || sourceIndex >= audioSamples.Count)
            return null;

        return new List<float>(audioSamples[sourceIndex]);
    }

    /// <summary>
    /// Cantidad de fuentes de audio registradas en este recorder.
    /// </summary>
    public int DiegeticSourceCount =>
        diegeticAudioSources != null ? diegeticAudioSources.Length : 0;

    // ── Privado ────────────────────────────────────────────────────────────

    private void CaptureFrame()
    {
        // ─ Captura visual ─
        RenderTexture.active = recordingTexture;
        Texture2D frame = new Texture2D(recordingTexture.width, recordingTexture.height,
                                        TextureFormat.RGB24, false);
        frame.ReadPixels(new Rect(0, 0, recordingTexture.width, recordingTexture.height), 0, 0);
        frame.Apply();
        RenderTexture.active = null;
        framesRecorded.Add(frame);

        // ─ Captura de audio diegético ─
        if (diegeticAudioSources == null || camViewpoint == null) return;

        Vector3 camPos = camViewpoint.position;
        Vector3 camFwd = camViewpoint.forward;

        for (int i = 0; i < diegeticAudioSources.Length; i++)
        {
            if (diegeticAudioSources[i] == null) continue;

            float sample = diegeticAudioSources[i].SampleVolume(camPos, camFwd);
            audioSamples[i].Add(sample);
        }
    }
}