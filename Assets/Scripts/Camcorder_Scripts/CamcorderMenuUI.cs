// CamcorderMenuUI.cs
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderMenuUI : MonoBehaviour
{
    [Header("Panels")]
    public GameObject noRecordingPanel;   // el panel azul — SIEMPRE activo
    public TextMeshProUGUI noRecordingText; // solo el texto se oculta
    public GameObject recordingsPanel;

    [Header("Recording Slots")]
    // Los 5 Image del RecordingsPanel (el thumbnail)
    public Image[] recordingSlots;

    // Un Image hijo de cada slot que actúa como marco de selección.
    // Ponelo como child de cada Image slot, mismo tamaño, color sólido, stretch.
    // En código lo controlamos nosotros.
    public Image[] selectionBorders;

    [Header("Colores")]
    public Color colorSelected = Color.white;
    public Color colorUnselected = new Color(0.45f, 0.45f, 0.45f, 1f);
    public Color borderColor = new Color(1f, 0.30f, 0.30f, 1f);

    private CamcorderStorage storage;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
    }

    public void UpdateUI(int selectedIndex)
    {
        List<RecordingData> recordings = storage.GetAllRecordings();
        bool hasRecordings = recordings.Count > 0;

        // El panel azul de fondo nunca se apaga
        noRecordingText.gameObject.SetActive(!hasRecordings);
        recordingsPanel.SetActive(hasRecordings);

        for (int i = 0; i < recordingSlots.Length; i++)
        {
            bool hasData = i < recordings.Count;
            bool isSelected = hasData && i == selectedIndex;

            // — Thumbnail —
            recordingSlots[i].gameObject.SetActive(hasData);

            if (hasData)
            {
                recordingSlots[i].sprite = TextureToSprite(recordings[i].frames[0]);
                recordingSlots[i].color = isSelected ? colorSelected : colorUnselected;
            }

            // — Borde de selección —
            if (selectionBorders != null && i < selectionBorders.Length)
            {
                selectionBorders[i].gameObject.SetActive(isSelected);
                selectionBorders[i].color = borderColor;
            }
        }
    }

    private Sprite TextureToSprite(Texture2D tex)
    {
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
    }
}