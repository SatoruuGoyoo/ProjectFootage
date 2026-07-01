using UnityEngine;

public class ShowFeedbackMessageStep : SequenceStep
{
    [TextArea(2, 5)]
    [SerializeField] private string message = "";
    [SerializeField] private float duration = -1f;

    protected override void OnExecute()
    {
        GameEvents.FeedbackMessage(message, UIPositioner.ScreenPosition.LowerCenter, duration);
        Complete();
    }
}