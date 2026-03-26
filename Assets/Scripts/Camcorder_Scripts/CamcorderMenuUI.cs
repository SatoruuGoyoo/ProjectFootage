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
        List<List<Texture2D>> recordings = storage.GetAllRecordings();

        for (int i = 0; i < recordingSlots.Length; i++)
        {
            if (i < recordings.Count)
            {
                // Tiene metraje — mostrá el primer frame como thumbnail
                recordingSlots[i].gameObject.SetActive(true);
                recordingSlots[i].sprite = TextureToSprite(recordings[i][0]);
                recordingSlots[i].color = (i == selectedIndex) ? selectedColor : unselectedColor;
            }
            else
            {
                // Sin metraje — ocultá el slot
                recordingSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private Sprite TextureToSprite(Texture2D texture)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }
}