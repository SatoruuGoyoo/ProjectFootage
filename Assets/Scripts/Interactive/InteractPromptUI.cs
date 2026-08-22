using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup container;
    [SerializeField] private UIPositioner positioner;
    [SerializeField] private UIPositioner.ScreenPosition defaultPosition = UIPositioner.ScreenPosition.LowerRight;

    [Header("Interact Icon")]
    [SerializeField] private Image interactIcon;
    [SerializeField] private Sprite defaultInteractSprite;
    [SerializeField] private Sprite defaultCancelSprite;

    [Header("Key Badge — assign sprites OR leave empty to use text")]
    [SerializeField] private Image keyImage;
    [SerializeField] private TMP_Text keyLabel;
    [SerializeField] private Sprite interactKeySprite;
    [SerializeField] private Sprite cancelKeySprite;
    [SerializeField] private string interactKeyText = "[E]";
    [SerializeField] private string cancelKeyText = "[F]";

    private bool _isVisible;
    private bool _hasPrompt;

    private void Awake()
    {
        _isVisible = true;
        RefreshKeyBadgeMode();
        SetVisible(false);
    }

    private void OnEnable()
    {
        GameEvents.OnInteractPromptShown += OnShown;
        GameEvents.OnInteractPromptHidden += OnHidden;
        UILayerManager.OnModalChanged += OnModalChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPromptShown -= OnShown;
        GameEvents.OnInteractPromptHidden -= OnHidden;
        UILayerManager.OnModalChanged -= OnModalChanged;
    }

    private void OnShown(InteractPrompt prompt)
    {
        _hasPrompt = true;
        positioner?.SetPosition(defaultPosition);

        if (interactIcon != null)
            interactIcon.sprite = prompt.Icon != null ? prompt.Icon : DefaultIconFor(prompt.Key);

        SetKeyBadge(prompt.Key);
        ApplyVisibility();
    }

    private void OnHidden()
    {
        _hasPrompt = false;
        ApplyVisibility();
    }

    private void OnModalChanged(bool modalOpen) => ApplyVisibility();

    private void ApplyVisibility() => SetVisible(_hasPrompt && !UILayerManager.IsModalOpen);

    private Sprite DefaultIconFor(InteractPromptKey key) =>
        key == InteractPromptKey.Cancel ? defaultCancelSprite : defaultInteractSprite;

    private void SetKeyBadge(InteractPromptKey key)
    {
        bool cancel = key == InteractPromptKey.Cancel;

        if (keyImage != null)
            keyImage.sprite = cancel ? cancelKeySprite : interactKeySprite;

        if (keyLabel != null)
            keyLabel.SetText(cancel ? cancelKeyText : interactKeyText);
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
        bool useSprite = interactKeySprite != null || cancelKeySprite != null;
        if (keyImage != null) keyImage.gameObject.SetActive(useSprite);
        if (keyLabel != null) keyLabel.gameObject.SetActive(!useSprite);
    }
}