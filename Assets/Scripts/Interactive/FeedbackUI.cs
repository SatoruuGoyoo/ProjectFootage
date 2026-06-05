using UnityEngine;
using TMPro;

/// <summary>
/// Displays temporary text messages (e.g. "Está cerrada", "Objeto recogido").
/// Listens to GameEvents.OnFeedbackMessage and auto-hides after a duration.
///
/// HIERARCHY:
/// FeedbackUI  (this script + CanvasGroup)
///  └─ MessageLabel  (TMP_Text)
/// </summary>
public class FeedbackUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text messageLabel;
    [SerializeField] private float displayDuration = 3f;

    private float _timer;
    private bool _isVisible;

    private void Awake() => ForceHide();

    private void OnEnable() => GameEvents.OnFeedbackMessage += OnFeedback;
    private void OnDisable() => GameEvents.OnFeedbackMessage -= OnFeedback;

    private void Update()
    {
        if (_timer <= 0f) return;
        _timer -= Time.deltaTime;
        if (_timer <= 0f) SetVisible(false);
    }

    private void OnFeedback(string message)
    {
        if (messageLabel != null) messageLabel.SetText(message);
        _timer = displayDuration;
        SetVisible(true);
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
}