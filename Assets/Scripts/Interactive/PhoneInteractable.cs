using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class PhoneInteractable : MonoBehaviour, IInteractable
{
    [Header("Cameras")]
    [SerializeField] private Camera _phoneCamera;
    [SerializeField] private Camera _playerCamera;

    

    [SerializeField] private GameObject _phoneUI;

    [Header("Settings")]
    [SerializeField] private string _openPrompt = "Revisar teléfono";
    [SerializeField] private string _closePrompt = "Cerrar teléfono";

    [Header("Events")]
    public UnityEvent OnPhoneOpened;
    public UnityEvent OnPhoneClosed;

    private bool _isOpen;

    public string PromptMessage => _isOpen ? _closePrompt : _openPrompt;
    public bool CanInteract => true;
    public bool IsOpen => _isOpen;

    private void Awake()
    {
        _phoneCamera.enabled = false;
        //SetButtonsEnabled(false);
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

    private void Open()
    {
        _isOpen = true;
        _phoneUI.SetActive(true);         
        OnPhoneOpened?.Invoke();
    }

    private void Close()
    {
        _isOpen = false;
        _phoneUI.SetActive(false);         
        OnPhoneClosed?.Invoke();
    }

    private void SetCameras(bool phoneActive)
    {
        _phoneCamera.enabled = phoneActive;
        _playerCamera.enabled = !phoneActive;
    }

    //private void SetButtonsEnabled(bool enabled)
    //{
    //    foreach (var btn in _buttons)
    //        btn.SetEnabled(enabled);
    //}
}