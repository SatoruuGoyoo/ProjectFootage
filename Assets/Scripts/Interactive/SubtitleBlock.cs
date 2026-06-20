using UnityEngine;
using TMPro;

/// <summary>
/// UI block that displays subtitle text while a radio (or anything) is playing.
/// Assign to a Canvas GO with a CanvasGroup + TMP_Text child.
///
/// HIERARCHY:
/// SubtitleBlock  (this script + CanvasGroup)
///  └─ Label      (TMP_Text)
/// </summary>
public class SubtitleBlock : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text label;

    private void Awake() => ForceHide();

    public void Show(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        if (label != null) label.SetText(text);
        SetVisible(true);
    }

    public void Hide() => SetVisible(false);

    private void SetVisible(bool visible)
    {
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }

    private void ForceHide()
    {
        if (container == null) return;
        container.alpha = 0f;
        container.interactable = false;
        container.blocksRaycasts = false;
    }
}