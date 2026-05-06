using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject noRecordingPanel;
    public TextMeshProUGUI noRecordingText;
    public GameObject recordingsPanel;

    [Header("Recording Slots")]
    public Image[] recordingSlots;
    public Image[] selectionBorders;

    [Header("Colores")]
    public Color colorSelected = Color.white;
    public Color colorUnselected = new Color(0.45f, 0.45f, 0.45f, 1f);
    public Color borderColor = new Color(1f, 0.30f, 0.30f, 1f);

    // Reutilizamos una sola Texture2D para los thumbnails
    // igual que hace VideoPlayback para los frames
    private Texture2D _thumbnailTexture;

    private CamcorderStorage _storage;

    private void Awake()
    {
        _storage = GetComponent<CamcorderStorage>();
    }

    public void UpdateUI(int selectedIndex)
    {
        IReadOnlyList<RecordingSession> recordings = _storage.GetAllRecordings();
        bool hasRecordings = recordings.Count > 0;

        noRecordingText.gameObject.SetActive(!hasRecordings);
        recordingsPanel.SetActive(hasRecordings);

        for (int i = 0; i < recordingSlots.Length; i++)
        {
            bool hasData = i < recordings.Count;
            bool isSelected = hasData && i == selectedIndex;

            recordingSlots[i].gameObject.SetActive(hasData);

            if (hasData)
            {
                // El thumbnail es el primer VideoFrame de la sesión
                // Lo convertimos a Sprite igual que antes, pero leyendo desde byte[]
                VideoFrame? firstFrame = recordings[i].GetFrameAtTime(0f);
                if (firstFrame.HasValue)
                    recordingSlots[i].sprite = FrameToSprite(firstFrame.Value);

                recordingSlots[i].color = isSelected ? colorSelected : colorUnselected;
            }

            if (selectionBorders != null && i < selectionBorders.Length)
            {
                selectionBorders[i].gameObject.SetActive(isSelected);
                selectionBorders[i].color = borderColor;
            }
        }
    }

    // ── Helpers ────────────────────────────────────────────────

    private Sprite FrameToSprite(VideoFrame frame)
    {
        // Reutilizamos la misma Texture2D para todos los thumbnails
        if (_thumbnailTexture == null)
            _thumbnailTexture = new Texture2D(640, 480, TextureFormat.RGB24, false);

        _thumbnailTexture.LoadRawTextureData(frame.PixelData);
        _thumbnailTexture.Apply();

        return Sprite.Create(
            _thumbnailTexture,
            new Rect(0, 0, _thumbnailTexture.width, _thumbnailTexture.height),
            Vector2.one * 0.5f
        );
    }

    private void OnDestroy()
    {
        if (_thumbnailTexture != null)
            Destroy(_thumbnailTexture);
    }
}