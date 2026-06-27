using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ReadableItem : Interactable
{
    [SerializeField] private Sprite sprite;
    [TextArea(3, 10)]
    [SerializeField] private string text = "";

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
        GameEvents.ReadableOpened(sprite, text, uiPosition);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
        GameEvents.InteractPromptActivated(PromptMessage);
    }

    private void Close()
    {
        _isReading = false;
        GameEvents.ReadableClosed();
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        GameEvents.InteractPromptDeactivated();
    }
}