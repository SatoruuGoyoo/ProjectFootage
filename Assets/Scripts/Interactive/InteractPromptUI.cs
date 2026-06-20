using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Shows a minimal interact hint (icon + key) when the player is near an interactable.
/// No text, no feedback — just the visual cue.
///
/// HIERARCHY:
/// InteractPromptUI  (this script + CanvasGroup)
///  └─ Container
///      ├─ InteractIcon   (Image — eye/hand sprite)
///      └─ KeyBadge
///          ├─ KeyImage   (Image — optional key sprite)
///          └─ KeyLabel   (TMP_Text — fallback "E")
/// </summary>
public class InteractPromptUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;

    [Header("Interact Icon")]
    [SerializeField] private Image interactIcon;

    [Header("Key Badge — assign sprite OR leave empty to use text")]
    [SerializeField] private Image keyImage;
    [SerializeField] private TMP_Text keyLabel;

    private bool _isVisible;

    private void Awake()
    {
        RefreshKeyBadgeMode();
        ForceHide();
    }

    private void OnEnable() => GameEvents.OnInteractPromptChanged += OnPromptChanged;
    private void OnDisable() => GameEvents.OnInteractPromptChanged -= OnPromptChanged;

    private void OnPromptChanged(string prompt)
    {
        SetVisible(!string.IsNullOrEmpty(prompt));
    }

    private void SetVisible(bool visible)
    {
        if (_isVisible == visible) return;
        _isVisible = visible;
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }

    private void ForceHide()
    {
        _isVisible = false;
        if (container == null) return;
        container.alpha = 0f;
        container.interactable = false;
        container.blocksRaycasts = false;
    }

    [ContextMenu("Refresh Key Badge Mode")]
    private void RefreshKeyBadgeMode()
    {
        bool useSprite = keyImage != null && keyImage.sprite != null;
        if (keyImage != null) keyImage.gameObject.SetActive(useSprite);
        if (keyLabel != null) keyLabel.gameObject.SetActive(!useSprite);
    }
}