using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject noRecordingPanel;
    [SerializeField] private TextMeshProUGUI noRecordingText;
    [SerializeField] private GameObject recordingsPanel;

    [Header("Recording Slots")]
    [SerializeField] private Image[] recordingSlots;
    [SerializeField] private Image[] selectionBorders;

    [Header("Colores")]
    [SerializeField] private Color colorSelected = Color.white;
    [SerializeField] private Color colorUnselected = new Color(0.45f, 0.45f, 0.45f, 1f);
    [SerializeField] private Color borderColor = new Color(1f, 0.30f, 0.30f, 1f);

    [Header("Textos")]
    [SerializeField] private string noRecordingsMessage = "NO RECORDINGS";
    [SerializeField] private string selectFootageMessage = "SELECT A FOOTAGE";

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

        noRecordingText.gameObject.SetActive(true);
        noRecordingText.text = hasRecordings ? selectFootageMessage : noRecordingsMessage;
        recordingsPanel.SetActive(hasRecordings);

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