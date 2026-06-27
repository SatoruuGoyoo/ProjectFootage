using TMPro;
using UnityEngine;

public class FeedbackUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private UIPositioner positioner;

    private float _timer;
    private bool _isVisible;

    private void Awake()
    {
        
        _isVisible = true;
        ForceHide();
    }

    private void OnEnable() => GameEvents.OnFeedbackMessage += OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackMessage -= OnFeedback;

    private void Update()
    {
        if (_timer <= 0f) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) Hide();
    }

    private void OnFeedback(string message, UIPositioner.ScreenPosition position)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.Feedback, ForceHide)) return;
        positioner?.SetPosition(position);
        if (messageLabel != null) messageLabel.SetText(message);
        _timer = displayDuration;
        SetVisible(true);
    }

    private void Hide()
    {
        UILayerManager.Release(UILayerManager.Layer.Feedback);
        SetVisible(false);
    }

    // Llamado por UILayerManager si algo de mayor prioridad nos desplaza.
    private void ForceHide()
    {
        _timer = 0f;
        UILayerManager.Release(UILayerManager.Layer.Feedback);
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