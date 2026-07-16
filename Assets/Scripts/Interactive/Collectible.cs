using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Collectible : Interactable
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

    public override string PromptMessage => prompt;
    public override bool CanInteract => !_collected;
    public override bool BlockMovement => false;

    private void OnEnable() => GameEvents.OnConfirmationClosed += OnConfirmationClosed;

    private void OnDisable()
    {
        GameEvents.OnConfirmationClosed -= OnConfirmationClosed;
        _pendingConfirmation = false;
    }

    public override void Interact()
    {
        Debug.Log($"[Collectible] Interact() frame={Time.frameCount} pendingConfirmation={_pendingConfirmation}");
        if (_collected) return;

        if (requiresConfirmation)
        {
            _pendingConfirmation = true;
            GameEvents.RequestConfirmation(confirmationText, OnConfirmed, OnDeclined, uiPosition);
        }
        else
        {
            Collect();
        }
    }

    private void OnConfirmed()
    {
        _pendingConfirmation = false;
        Collect();
    }

    private void OnDeclined() => _pendingConfirmation = false;

    private void OnConfirmationClosed() => _pendingConfirmation = false;

    private void Collect()
    {
        if (_collected) return;
        _collected = true;

        ItemRegistry.Instance.Collect(itemId);
        GameEvents.ItemCollected(itemId);
        GameEvents.FeedbackMessage(feedbackMessage, uiPosition);

        RuntimeManager.PlayOneShot(collectSound, transform.position);

        gameObject.SetActive(false);
    }
}