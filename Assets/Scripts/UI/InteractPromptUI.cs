using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float feedbackDuration = 3f;

    private string currentPrompt = "";
    private float feedbackTimer;

    private void Awake()
    {
        if (container != null) container.SetActive(false);
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
        if (feedbackTimer <= 0f) ShowText(currentPrompt); // vuelve al prompt anterior
    }

    private void OnPromptChanged(string prompt)
    {
        currentPrompt = prompt;
        if (feedbackTimer <= 0f) ShowText(prompt); // solo actualiza si no hay feedback activo
    }

    private void OnFeedback(string message)
    {
        feedbackTimer = feedbackDuration;
        ShowText(message);
    }

    private void ShowText(string text)
    {
        bool show = !string.IsNullOrEmpty(text);
        if (container != null) container.SetActive(show);
        if (label != null) label.text = text;
    }
}