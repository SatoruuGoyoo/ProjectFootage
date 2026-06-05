using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Panel that slides in from the right when the player reads a note/flyer.
/// 
/// HIERARCHY:
/// ReadableUI  (this script + CanvasGroup)
///  └─ Container  (anchored to right side)
///      ├─ Gradient   (Image — your black-to-transparent sprite)
///      ├─ ItemSprite (Image — the readable's sprite)
///      └─ TextField  (TMP_Text — the note text)
/// </summary>
public class ReadableUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private Image itemSprite;
    [SerializeField] private TMP_Text textField;

    private void Awake() => ForceHide();

    private void OnEnable()
    {
        GameEvents.OnReadableOpened += OnOpened;
        GameEvents.OnReadableClosed += OnClosed;
    }

    private void OnDisable()
    {
        GameEvents.OnReadableOpened -= OnOpened;
        GameEvents.OnReadableClosed -= OnClosed;
    }

    private void OnOpened(Sprite sprite, string text)
    {
        if (itemSprite != null) itemSprite.sprite = sprite;
        if (textField != null) textField.SetText(text);
        SetVisible(true);
    }

    private void OnClosed() => SetVisible(false);

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