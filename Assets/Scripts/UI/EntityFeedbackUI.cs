using TMPro;
using UnityEngine;

public class EntityFeedbackUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private UIPositioner positioner;
    [SerializeField] private UIPositioner.ScreenPosition defaultPosition = UIPositioner.ScreenPosition.LowerCenter;

    private float _timer;
    private bool _isVisible;

    private void Awake()
    {
        _isVisible = true;
        ForceHide();
    }

    private void OnEnable() => GameEvents.OnEntityFeedbackMessage += OnFeedback;
    private void OnDisable() => GameEvents.OnEntityFeedbackMessage -= OnFeedback;

    private void Update()
    {
        if (_timer <= 0f) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) Hide();
    }

    private void OnFeedback(string message, UIPositioner.ScreenPosition position, float duration)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.EntityFeedback, ForceHide)) return;
        positioner?.SetPosition(position != UIPositioner.ScreenPosition.LowerLeft ? position : defaultPosition);
        if (messageLabel != null) messageLabel.SetText(message);
        _timer = duration > 0f ? duration : defaultDisplayDuration;
        SetVisible(true);
    }

    private void Hide()
    {
        UILayerManager.Release(UILayerManager.Layer.EntityFeedback);
        SetVisible(false);
    }

    private void ForceHide()
    {
        _timer = 0f;
        UILayerManager.Release(UILayerManager.Layer.EntityFeedback);
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