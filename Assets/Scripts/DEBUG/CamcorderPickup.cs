using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Collider))]
public class CamcorderPickup : Interactable
{
    [Header("References")]
    [SerializeField] private GameObject camcorderSystem;
    [SerializeField] private GameObject camcorderModel;

    [Header("Prompt")]
    [SerializeField] private string pickupPrompt = "camcorder";

    [Header("Feedback")]
    [SerializeField] private string postPickupMessage = "";

    [Header("Audio")]
    [SerializeField] private EventReference pickupSound;

    private bool _taken;

    public override string PromptMessage => pickupPrompt;
    public override bool CanInteract => !_taken;
    public override bool BlockMovement => false;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
        if (camcorderSystem != null) camcorderSystem.SetActive(false);
        if (camcorderModel != null) camcorderModel.SetActive(false);
    }

    public override void Interact()
    {
        if (_taken) return;
        _taken = true;

        if (!pickupSound.IsNull)
            RuntimeManager.PlayOneShot(pickupSound, transform.position);

        if (camcorderSystem != null) camcorderSystem.SetActive(true);
        if (camcorderModel != null) camcorderModel.SetActive(true);

        GameEvents.CamcorderPickedUp();

        if (!string.IsNullOrEmpty(postPickupMessage))
            GameEvents.FeedbackMessage(postPickupMessage, uiPosition);

        gameObject.SetActive(false);
    }
}