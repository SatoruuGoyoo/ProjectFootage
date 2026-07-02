using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReadableItem : Interactable
{
    [SerializeField] private Sprite sprite;
    [TextArea(3, 10)]
    [SerializeField] private string text = "";

    [SerializeField] private EventReference openSound;
    [SerializeField] private EventReference closeSound;

    private bool _isReading;

    public override string PromptMessage => "readable";
    public override bool CanInteract => true;
    public override bool BlockMovement => true;

    private void OnDisable()
    {
        if (_isReading) Close();
    }

    public override void Interact()
    {
        if (_isReading) Close();
        else Open();
    }

    private void Open()
    {
        _isReading = true;
        RuntimeManager.PlayOneShot(openSound, transform.position);
        GameEvents.ReadableOpened(sprite, text, uiPosition);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
        GameEvents.InteractPromptActivated(PromptMessage);
    }

    private void Close()
    {
        _isReading = false;
        if (!closeSound.IsNull)
            RuntimeManager.PlayOneShot(closeSound, transform.position);
        GameEvents.ReadableClosed();
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        GameEvents.InteractPromptDeactivated();
    }
}