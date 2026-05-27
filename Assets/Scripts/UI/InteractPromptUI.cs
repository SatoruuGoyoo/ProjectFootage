using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float feedbackDuration = 3f;

    private string currentPrompt = "";
    private float feedbackTimer;
    private bool _isVisible;

    private void Awake()
    {
        SetVisible(false);
        if (label != null) label.SetText("");
    }

    private void OnEnable()
    {
        GameEvents.OnInteractPromptChanged += OnPromptChanged;
        GameEvents.OnFeedbackMessage += OnFeedback;
    }

    private void OnDisable()
    {
        GameEvents.OnInteractPromptChanged -= OnPromptChanged;
        GameEvents.OnFeedbackMessage -= OnFeedback;
    }

    private void Update()
    {
        if (feedbackTimer <= 0f) return;
        feedbackTimer -= Time.deltaTime;
        if (feedbackTimer <= 0f) ShowText(currentPrompt);
    }

    private void OnPromptChanged(string prompt)
    {
        currentPrompt = prompt;
        if (feedbackTimer <= 0f) ShowText(prompt);
    }

    private void OnFeedback(string message)
    {
        feedbackTimer = feedbackDuration;
        ShowText(message);
    }

    private void ShowText(string text)
    {
        bool show = !string.IsNullOrEmpty(text);
        SetVisible(show);
        if (label != null) label.SetText(text);
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