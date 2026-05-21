using UnityEngine;
using FMODUnity;

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
    [SerializeField] private float speed = 120f;

    [Header("Audio")]
    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;
    [SerializeField] private EventReference lockedSound;

    private bool isOpen;
    private Quaternion closedRot;
    private Quaternion openRot;

    private bool IsLocked => !string.IsNullOrEmpty(requiredItemId)
        && (ItemRegistry.Instance == null || !ItemRegistry.Instance.Has(requiredItemId));

    // Actualiza el prompt cada frame según el estado actual
    public string PromptMessage => IsLocked ? lockedPrompt : (isOpen ? closePrompt : openPrompt);
    public bool CanInteract => true;

    private void Awake()
    {
        if (pivot == null) pivot = transform;
        closedRot = pivot.localRotation;
        RecalculateOpenRot(1f); // por defecto abre hacia un lado
    }

    private void RecalculateOpenRot(float direction)
    {
        openRot = closedRot * Quaternion.Euler(0f, openAngle * direction, 0f);
    }

    public void Interact()
    {
        if (IsLocked)
        {
            GameEvents.FeedbackMessage(lockedFeedback);
            GameEvents.InteractPromptChanged(lockedPrompt);
            if (!lockedSound.IsNull) RuntimeManager.PlayOneShot(lockedSound, transform.position);
            return;
        }
        Toggle();
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    public void Open(Transform interactor = null)
    {
        if (IsLocked) return;

        // Abre hacia el lado opuesto al interactor
        if (interactor != null)
        {
            Vector3 toInteractor = interactor.position - pivot.position;
            float dot = Vector3.Dot(toInteractor, pivot.forward);
            RecalculateOpenRot(dot > 0 ? -1f : 1f);
        }

        isOpen = true;
        GameEvents.InteractPromptChanged(closePrompt);
        if (!openSound.IsNull) RuntimeManager.PlayOneShot(openSound, transform.position);
    }

    public void Close()
    {
        isOpen = false;
        GameEvents.InteractPromptChanged(openPrompt);
        if (!closeSound.IsNull) RuntimeManager.PlayOneShot(closeSound, transform.position);
    }

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