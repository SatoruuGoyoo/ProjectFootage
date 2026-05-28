using UnityEngine;
using TMPro;

public class SubtitleBlock : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        if (label != null) label.SetText(text);
        if (group != null)
        {
            group.alpha = 1f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    public void Hide()
    {
        if (label != null) label.SetText("");
        if (group != null) group.alpha = 0f;
    }
}