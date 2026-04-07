using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderMenuUI : MonoBehaviour
{
    [Header("Setup")]
    public Image[] recordingSlots; // 5 slots
    public Color selectedColor = Color.white;
    public Color unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private CamcorderStorage storage;
    private CamcorderMenuController menuController;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
        menuController = GetComponent<CamcorderMenuController>();
    }

    public void UpdateUI(int selectedIndex)
    {
        List<RecordingData> recordings = storage.GetAllRecordings();

        for (int i = 0; i < recordingSlots.Length; i++)
        {
            if (i < recordings.Count)
            {
                recordingSlots[i].gameObject.SetActive(true);
                recordingSlots[i].sprite = TextureToSprite(recordings[i].frames[0]); // .frames[0]
                recordingSlots[i].color = (i == selectedIndex) ? selectedColor : unselectedColor;
            }
            else
            {
                recordingSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
}