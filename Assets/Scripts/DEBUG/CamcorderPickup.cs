using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Collider))]
public class CamcorderPickup : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private GameObject camcorderSystem;
    [SerializeField] private GameObject camcorderModel;

    [Header("Feedback")]
    [SerializeField] private string postPickupMessage = "";

    [Header("Audio")]
    [SerializeField] private EventReference pickupSound;

    // ── IInteractable ─────────────────────────────────────────────────────────
    public string PromptMessage => "camcorder";
    public bool CanInteract => true;
    public bool BlockMovement => false;

    // ── Unity ────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Ensure trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;

        if (camcorderSystem != null) camcorderSystem.SetActive(false);
        if (camcorderModel != null) camcorderModel.SetActive(false);
    }

    // ── IInteractable ─────────────────────────────────────────────────────────

    public void Interact()
    {
        if (!pickupSound.IsNull)
            RuntimeManager.PlayOneShot(pickupSound, transform.position);

        if (camcorderSystem != null) camcorderSystem.SetActive(true);
        if (camcorderModel != null) camcorderModel.SetActive(true);

        GameEvents.CamcorderPickedUp();
        GameEvents.FeedbackMessage(postPickupMessage);

        gameObject.SetActive(false);
    }
}