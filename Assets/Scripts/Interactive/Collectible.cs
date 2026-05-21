using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId;
    [SerializeField] private string prompt = "";
    [SerializeField] private string feedbackMessage = "";
    [SerializeField] private EventReference collectSound;

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

        RuntimeManager.PlayOneShot(collectSound, transform.position);

        gameObject.SetActive(false);
    }
}