using UnityEngine;
using TMPro;

public class InteractPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        GameEvents.OnInteractPromptChanged += OnPromptChanged;
        if (container != null) container.SetActive(false);
    }

    private void OnDisable() => GameEvents.OnInteractPromptChanged -= OnPromptChanged;

    private void OnPromptChanged(string prompt)
    {
        bool show = !string.IsNullOrEmpty(prompt);
        if (container != null) container.SetActive(show);
        if (show && label != null) label.text = prompt;
    }
}