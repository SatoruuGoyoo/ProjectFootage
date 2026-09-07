using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReadableItem : Interactable
{
    [SerializeField] private Sprite sprite;
    [TextArea(3, 10)]
    [SerializeField] private string[] pages = new[] { "" };

    [Header("Prompt")]
    [SerializeField] private string openPrompt = "leer";
    [SerializeField] private string closePrompt = "cerrar";

    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;

    private bool _isReading;

    public override string PromptMessage => _isReading ? closePrompt : openPrompt;
    public override bool CanInteract => true;
    public override bool IsActive => _isReading;
    public override bool BlockMovement => true;

    public override void Interact()
    {
        if (_isReading) Close();
        else Open();
    }

    private void OnDisable()
    {
        if (_isReading) Close();
    }

    private void Open()
    {
        _isReading = true;
        if (!openSound.IsNull) RuntimeManager.PlayOneShot(openSound, transform.position);
        GameEvents.ReadableOpened(sprite, pages, uiPosition);
        EnterInteractionMode();
    }

    private void Close()
    {
        _isReading = false;
        if (!closeSound.IsNull) RuntimeManager.PlayOneShot(closeSound, transform.position);
        GameEvents.ReadableClosed();
        ExitInteractionMode();
    }
}