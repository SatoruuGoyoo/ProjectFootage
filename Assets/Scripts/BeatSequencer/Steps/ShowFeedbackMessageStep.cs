using UnityEngine;

public class ShowFeedbackMessageStep : SequenceStep
{
    [TextArea(2, 5)]
    [SerializeField] private string message = "";

    protected override void OnExecute()
    {
        GameEvents.FeedbackMessage(message);
        Complete();
    }
}