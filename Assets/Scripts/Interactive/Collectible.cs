using UnityEngine;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId;
    [SerializeField] private string prompt = "Recoger";
    [SerializeField] private string feedbackMessage = "Objeto recogido";

    private bool _collected;

    public string PromptMessage => prompt;
    public bool CanInteract => !_collected;

    public void Interact()
    {
        if (_collected) return;
        _collected = true;

        ItemRegistry.Instance.Collect(itemId);
        GameEvents.ItemCollected(itemId);
        GameEvents.FeedbackMessage(feedbackMessage);

        gameObject.SetActive(false);
    }
}