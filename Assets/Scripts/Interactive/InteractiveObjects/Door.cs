using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [Header("Lock")]
    [SerializeField] private string requiredItemId;
    [SerializeField] private string lockedPrompt = "Need the doorknob to open";
    [SerializeField] private string openPrompt = "Open";
    [SerializeField] private string closePrompt = "Close";
    [SerializeField] private string lockedFeedback = "It's locked";

    [Header("Motion")]
    [SerializeField] private Transform pivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float speed = 2f;

    private bool isOpen;
    private Quaternion closedRot;
    private Quaternion openRot;

    private bool IsLocked => !string.IsNullOrEmpty(requiredItemId)
        && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId));

    public string PromptMessage => IsLocked ? lockedPrompt : (isOpen ? closePrompt : openPrompt);
    public bool CanInteract => true;

    private void Awake()
    {
        if (pivot == null) pivot = transform;
        closedRot = pivot.localRotation;
        openRot = closedRot * Quaternion.Euler(0f, openAngle, 0f);
    }

    public void Interact()
    {
        if (IsLocked)
        {
            GameEvents.FeedbackMessage(lockedFeedback);
            return;
        }
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
    }

    public void Close() => isOpen = false;

    private void Update()
    {
        Quaternion target = isOpen ? openRot : closedRot;
        pivot.localRotation = Quaternion.Slerp(pivot.localRotation, target, Time.deltaTime * speed);
    }
}