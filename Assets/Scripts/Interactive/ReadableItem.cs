using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReadableItem : MonoBehaviour, IInteractable
{
    [SerializeField] private Sprite sprite;
    [TextArea(3, 10)]
    [SerializeField] private string text = "";

    private bool _isReading;

    public string PromptMessage => "readable";
    public bool CanInteract => true;

    private void OnDisable()
    {
        if (_isReading) Close();
    }

    public void Interact()
    {
        if (_isReading) Close();
        else Open();
    }

    private void Open()
    {
        _isReading = true;
        GameEvents.ReadableOpened(sprite, text);
    }

    private void Close()
    {
        _isReading = false;
        GameEvents.ReadableClosed();
    }
}