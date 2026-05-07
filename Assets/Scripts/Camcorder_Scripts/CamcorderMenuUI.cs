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

    // Una Texture2D por slot — así cada thumbnail es independiente
    private Texture2D[] _thumbnailTextures;

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

        // Inicializamos el array de texturas si no existe
        if (_thumbnailTextures == null || _thumbnailTextures.Length != recordingSlots.Length)
            _thumbnailTextures = new Texture2D[recordingSlots.Length];

        for (int i = 0; i < recordingSlots.Length; i++)
        {
            bool hasData = i < recordings.Count;
            bool isSelected = hasData && i == selectedIndex;

            recordingSlots[i].gameObject.SetActive(hasData);

            if (hasData)
            {
                VideoFrame? firstFrame = recordings[i].GetFrameAtTime(0f);
                if (firstFrame.HasValue)
                    recordingSlots[i].sprite = FrameToSprite(i, firstFrame.Value);

                recordingSlots[i].color = isSelected ? colorSelected : colorUnselected;
            }

            if (selectionBorders != null && i < selectionBorders.Length)
            {
                selectionBorders[i].gameObject.SetActive(isSelected);
                selectionBorders[i].color = borderColor;
            }
        }
    }

    // Cada slot tiene su propio índice y su propia Texture2D
    private Sprite FrameToSprite(int slotIndex, VideoFrame frame)
    {
        if (_thumbnailTextures[slotIndex] == null)
            _thumbnailTextures[slotIndex] = new Texture2D(640, 480, TextureFormat.RGB24, false);

        _thumbnailTextures[slotIndex].LoadRawTextureData(frame.PixelData);
        _thumbnailTextures[slotIndex].Apply();

        return Sprite.Create(
            _thumbnailTextures[slotIndex],
            new Rect(0, 0, 640, 480),
            Vector2.one * 0.5f
        );
    }

    private void OnDestroy()
    {
        if (_thumbnailTextures == null) return;
        foreach (var tex in _thumbnailTextures)
            if (tex != null) Destroy(tex);
    }
}