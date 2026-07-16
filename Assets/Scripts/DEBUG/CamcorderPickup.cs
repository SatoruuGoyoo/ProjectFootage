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

    [Header("Prompt Icon")]
    [SerializeField] private Sprite promptIcon;

    public bool IsActive => false;
    public Sprite ActiveIcon => null;

    public string PromptMessage => "camcorder";
    public bool CanInteract => true;
    public bool BlockMovement => false;
    public Sprite PromptIcon => promptIcon;

    private void Awake()
    {
        // Ensure trigger
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
        if (camcorderSystem != null) camcorderSystem.SetActive(false);
        if (camcorderModel != null) camcorderModel.SetActive(false);
    }

    public void Interact()
    {
        if (!pickupSound.IsNull)
            RuntimeManager.PlayOneShot(pickupSound, transform.position);
        if (camcorderSystem != null) camcorderSystem.SetActive(true);
        if (camcorderModel != null) camcorderModel.SetActive(true);
        GameEvents.CamcorderPickedUp();
        if (!string.IsNullOrEmpty(postPickupMessage))
        {
            GameEvents.FeedbackMessage(postPickupMessage);
        }
        gameObject.SetActive(false);
    }
}