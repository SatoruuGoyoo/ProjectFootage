using TMPro;
using UnityEngine;

public class TutorialPromptUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private UIPositioner positioner;

    private bool _isVisible;

    private void Awake()
    {
        _isVisible = true;
        ForceHide();
    }

    private void OnEnable()
    {
        GameEvents.OnTutorialPromptShown += OnShown;
        GameEvents.OnTutorialPromptHidden += OnHidden;
    }

    private void OnDisable()
    {
        GameEvents.OnTutorialPromptShown -= OnShown;
        GameEvents.OnTutorialPromptHidden -= OnHidden;
    }

    private void OnShown(string message, UIPositioner.ScreenPosition position)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.TutorialPrompt, ForceHide)) return;
        positioner?.SetPosition(position);
        if (messageLabel != null) messageLabel.SetText(message);
        SetVisible(true);
    }

    private void OnHidden()
    {
        UILayerManager.Release(UILayerManager.Layer.TutorialPrompt);
        SetVisible(false);
    }

    private void ForceHide()
    {
        UILayerManager.Release(UILayerManager.Layer.TutorialPrompt);
        SetVisible(false);
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
}