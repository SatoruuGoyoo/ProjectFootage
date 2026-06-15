using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Collectible : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemId;
    [SerializeField] private string prompt = "";
    [SerializeField] private string feedbackMessage = "";
    [SerializeField] private EventReference collectSound;

    [Header("Confirmation")]
    [SerializeField] private bool requiresConfirmation = false;
    [SerializeField] private string confirmationText = "";

    private bool _collected;
    private bool _pendingConfirmation;

    public string PromptMessage => prompt;
    public bool CanInteract => !_collected;
    public bool BlockMovement => false;


    private void OnEnable()
    {
        GameEvents.OnConfirmationClosed += OnConfirmationClosed;
    }

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        _pendingConfirmation = false;
    }

    public void Interact()
    {
        if (_collected) return;

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined);
        }
        else
        {
            Collect();
        }
    }

    // ── Confirmation callbacks ────────────────────────────────────────────────

    private void OnConfirmed()
    {
        _pendingConfirmation = false;
        Collect();
    }

    private void OnDeclined()
    {
        _pendingConfirmation = false;
    }

    /// <summary>Called when the panel closes externally (player walked away).</summary>
    private void OnConfirmationClosed()
    {
        _pendingConfirmation = false;
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void Collect()
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