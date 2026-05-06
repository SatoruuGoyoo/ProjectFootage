using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsabilidad única: mostrar el frame correcto en pantalla
/// según el tiempo actual del PlaybackClock.
/// No sabe nada de audio ni de FMOD.
/// </summary>
[RequireComponent(typeof(PlaybackClock))]
public class VideoPlayback : MonoBehaviour
{
    [Header("Setup")]
    public RawImage displayImage;     // el RawImage donde se ve el footage
    public GameObject playbackPanel;  // el panel que contiene el display

    private PlaybackClock _clock;
    private RecordingSession _session;

    // Reutilizamos UNA sola Texture2D durante todo el playback
    // En vez de crear una nueva por frame como antes
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

    // ── API pública ────────────────────────────────────────────

    public void Load(RecordingSession session)
    {
        _session = session;

        // Creamos la textura UNA sola vez con el tamaño correcto
        // La vamos a reutilizar en cada frame — cero allocations durante playback
        if (_displayTexture == null)
            _displayTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);

        displayImage.texture = _displayTexture;
    }

    // ── Respuestas al Clock ────────────────────────────────────

    private void OnPlay()
    {
        playbackPanel.SetActive(true);
        ShowFrameAtTime(_clock.CurrentTime);
    }

    private void OnPause()
    {
        // El frame actual queda congelado en pantalla — no hay que hacer nada
    }

    private void OnStop()
    {
        playbackPanel.SetActive(false);
        _session = null;
    }

    private void OnSeek(float time)
    {
        // El jugador hizo RFF — actualizamos el frame inmediatamente
        ShowFrameAtTime(time);
    }

    // ── Loop ──────────────────────────────────────────────────

    private void Update()
    {
        // Solo actualizamos si el clock está corriendo
        // OnSeek ya maneja los saltos, Update maneja la reproducción normal
        if (_clock.IsPlaying)
            ShowFrameAtTime(_clock.CurrentTime);
    }

    // ── Lógica de display ─────────────────────────────────────

    private void ShowFrameAtTime(float time)
    {
        if (_session == null) return;

        VideoFrame? frame = _session.GetFrameAtTime(time);
        if (frame == null) return;

        // LoadRawTextureData + Apply es la forma más eficiente de
        // cargar bytes crudos en una Texture2D existente sin crear garbage
        _displayTexture.LoadRawTextureData(frame.Value.PixelData);
        _displayTexture.Apply();
    }

    private void OnDestroy()
    {
        if (_displayTexture != null)
            Destroy(_displayTexture);
    }
}