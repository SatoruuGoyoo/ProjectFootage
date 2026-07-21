using UnityEngine;

public class EntityFeedbackStep : SequenceStep
{
    [TextArea(2, 5)]
    [SerializeField] private string message = "";
    [SerializeField] private float duration = -1f;

    protected override void OnExecute()
    {
        GameEvents.EntityFeedbackMessage(message, UIPositioner.ScreenPosition.MiddleCenter, duration);
        Complete();
    }
}
