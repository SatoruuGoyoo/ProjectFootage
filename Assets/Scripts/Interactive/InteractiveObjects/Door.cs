using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private bool startLocked = false;
    [SerializeField] private bool playerCanToggle = true;
    [SerializeField] private string lockedPrompt = "";
    [SerializeField] private string openPrompt = "";
    [SerializeField] private string closePrompt = "";
    [SerializeField] private string lockedFeedback = "";

    [Header("State")]
    [SerializeField] private bool startOpen = false;

    [Header("Motion")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 2f;

    [Header("Audio")]

    [SerializeField] private EventReference openSound;

    [SerializeField] private EventReference closeSound;

    [SerializeField] private EventReference lockedSound;
    private bool isOpen;
    private bool manualLock;
    private Quaternion closedRot;
    private Quaternion openRot;

    private bool IsLocked => manualLock
        || (!string.IsNullOrEmpty(requiredItemId)
            && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId)));

    public string PromptMessage
    {
        get
        {
            if (IsLocked) return lockedPrompt;
            if (!playerCanToggle) return "";
            return isOpen ? closePrompt : openPrompt;
        }
    }

    public bool CanInteract => IsLocked || playerCanToggle;

    private void Awake()
    {
        if (pivot == null) pivot = transform;
        closedRot = pivot.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
        manualLock = startLocked;

        isOpen = startOpen;
        pivot.localRotation = isOpen ? openRot : closedRot;
    }

    public void Interact()
    {
        if (IsLocked)
        {
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, transform.position);
            GameEvents.FeedbackMessage(lockedFeedback);
            return;
        }
        if (!playerCanToggle) return;
        Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsLocked) return;
        isOpen = true;
        if (!openSound.IsNull) RuntimeManager.PlayOneShot(openSound, transform.position);
    }
    public void Close()
    {
        isOpen = false;
        if (!closeSound.IsNull) RuntimeManager.PlayOneShot(closeSound, transform.position);
    }
    public void Lock() => manualLock = true;
    public void Unlock() => manualLock = false;

    private void Update()
    {
        Quaternion target = isOpen ? openRot : closedRot;
        pivot.localRotation = Quaternion.RotateTowards(
            pivot.localRotation,
            target,
            speed * Time.deltaTime
        );
    }
}