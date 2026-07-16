using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private UIPositioner positioner;
    [SerializeField] private UIPositioner.ScreenPosition defaultPosition = UIPositioner.ScreenPosition.LowerCenter;

    [Header("Interact Icon")]
    [SerializeField] private Image interactIcon;

    [Header("Key Badge — assign sprite OR leave empty to use text")]
    [SerializeField] private Image keyImage;
    [SerializeField] private TMP_Text keyLabel;

    [Header("Key Badge — Active State (closes with Cancel)")]
    [SerializeField] private Sprite activeKeySprite;
    [SerializeField] private string activeKeyText = "[F]";

    [Header("Icon Sprites")]
    [SerializeField] private Sprite proximitySprite;
    [SerializeField] private Sprite closeSprite;

    private bool _isVisible;
    private bool _isActive;
    private Sprite _proximityKeySprite;
    private string _proximityKeyText;

    private void Awake()
    {
        _isVisible = true;
        _proximityKeySprite = keyImage != null ? keyImage.sprite : null;
        _proximityKeyText = keyLabel != null ? keyLabel.text : "";
        RefreshKeyBadgeMode();
        ForceHide();
    }

    private void OnEnable()
    {
        GameEvents.OnInteractPromptChanged += OnPromptChanged;
        GameEvents.OnInteractPromptActivated += OnActivated;
        GameEvents.OnInteractPromptDeactivated += OnDeactivated;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPromptChanged -= OnPromptChanged;
        GameEvents.OnInteractPromptActivated -= OnActivated;
        GameEvents.OnInteractPromptDeactivated -= OnDeactivated;
    }

    private void OnPromptChanged(string prompt, Sprite icon)
    {
        if (_isActive) return;

        if (string.IsNullOrEmpty(prompt))
        {
            Hide();
            return;
        }

        if (!UILayerManager.TryShow(UILayerManager.Layer.InteractPrompt, ForceHide)) return;

        positioner?.SetPosition(defaultPosition);
        if (interactIcon != null)
            interactIcon.sprite = icon != null ? icon : proximitySprite;

        SetKeyBadge(active: false);
        SetVisible(true);
    }

    private void OnActivated(string promptType, Sprite icon)
    {
        _isActive = true;
        UILayerManager.Release(UILayerManager.Layer.InteractPrompt);
        positioner?.SetPosition(defaultPosition);
        if (interactIcon != null)
            interactIcon.sprite = icon != null ? icon : closeSprite;

        SetKeyBadge(active: true);
        SetVisible(true);
    }

    private void OnDeactivated()
    {
        _isActive = false;
        SetVisible(false);
    }

    private void Hide()
    {
        UILayerManager.Release(UILayerManager.Layer.InteractPrompt);
        SetVisible(false);
    }

    private void ForceHide()
    {
        if (_isActive) return;
        UILayerManager.Release(UILayerManager.Layer.InteractPrompt);
        SetVisible(false);
    }

    private void SetKeyBadge(bool active)
    {
        if (keyImage != null)
            keyImage.sprite = (active && activeKeySprite != null) ? activeKeySprite : _proximityKeySprite;

        if (keyLabel != null)
            keyLabel.SetText((active && !string.IsNullOrEmpty(activeKeyText)) ? activeKeyText : _proximityKeyText);
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

    [ContextMenu("Refresh Key Badge Mode")]
    private void RefreshKeyBadgeMode()
    {
        bool useSprite = keyImage != null && keyImage.sprite != null;
        if (keyImage != null) keyImage.gameObject.SetActive(useSprite);
        if (keyLabel != null) keyLabel.gameObject.SetActive(!useSprite);
    }
}