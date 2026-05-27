using UnityEngine;
using UnityEngine.Events;
using FMODUnity;

[RequireComponent(typeof(Collider))]
public class PhoneInteractable : MonoBehaviour, IInteractable
{
    [Header("Cameras")]
    [SerializeField] private Camera _phoneCamera;
    [SerializeField] private Camera _playerCamera;

    [Header("FMOD")]
    [SerializeField] private EventReference putUpPhoneReference;
    [SerializeField] private EventReference putDownPhoneReference;
    [SerializeField] private EventReference markNumberReference;

    [SerializeField] private GameObject _phoneUI;

    [Header("Settings")]
    [SerializeField] private string _openPrompt = "";
    [SerializeField] private string _closePrompt = "";

    [Header("Events")]
    public UnityEvent OnPhoneOpened;
    public UnityEvent OnPhoneClosed;

    private bool _isOpen;

    public string PromptMessage => _isOpen ? _closePrompt : _openPrompt;
    public bool CanInteract => true;
    public bool IsOpen => _isOpen;
    public EventReference MarkNumberReference => markNumberReference;

    private void Awake()
    {
        _phoneCamera.enabled = false;
    }

    public void Interact()
    {
        if (_isOpen) Close();
        else Open();
    }

    public void ForceClose()
    {
        if (_isOpen) Close();
    }

    public void PlayMarkNumber()
    {
        if (markNumberReference.IsNull) return;
        RuntimeManager.PlayOneShot(markNumberReference, transform.position);
    }

    private void Open()
    {
        _isOpen = true;
        _phoneUI.SetActive(true);
        if (!putUpPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putUpPhoneReference, transform.position);
        OnPhoneOpened?.Invoke();
    }

    private void Close()
    {
        _isOpen = false;
        _phoneUI.SetActive(false);
        if (!putDownPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putDownPhoneReference, transform.position);
        OnPhoneClosed?.Invoke();
    }

    private void SetCameras(bool phoneActive)
    {
        _phoneCamera.enabled = phoneActive;
        _playerCamera.enabled = !phoneActive;
    }
}