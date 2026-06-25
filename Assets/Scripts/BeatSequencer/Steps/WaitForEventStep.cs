using UnityEngine;

public class WaitForEventStep : SequenceStep
{
    public enum EventType
    {
        CamcorderPickedUp,
        RecordingStarted,
        RecordingStopped,
        RecordableEventStarted,
        RecordableEventCompleted,
        RecordableEventInterrupted,
        RecordableEventWatched
    }

    [SerializeField] private EventType eventToWait;
    [Tooltip("Si el evento tiene un id (RecordableEvent), filtra por este id. Vacío = cualquier id.")]
    [SerializeField] private string filterId = "";

    protected override void OnExecute()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        switch (eventToWait)
        {
            case EventType.CamcorderPickedUp:
                GameEvents.OnCamcorderPickedUp += HandleSimpleEvent;
                break;
            case EventType.RecordingStarted:
                GameEvents.OnRecordingStarted += HandleRecordingStartedEvent;
                break;
            case EventType.RecordingStopped:
                GameEvents.OnRecordingStopped += HandleSimpleEvent;
                break;
            case EventType.RecordableEventStarted:
                GameEvents.OnRecordableEventStarted += HandleIdEvent;
                break;
            case EventType.RecordableEventCompleted:
                GameEvents.OnRecordableEventCompleted += HandleIdEvent;
                break;
            case EventType.RecordableEventInterrupted:
                GameEvents.OnRecordableEventInterrupted += HandleIdEvent;
                break;
            case EventType.RecordableEventWatched:
                GameEvents.OnRecordableEventWatched += HandleIdEvent;
                break;
        }
    }

    private void Unsubscribe()
    {
        switch (eventToWait)
        {
            case EventType.CamcorderPickedUp:
                GameEvents.OnCamcorderPickedUp -= HandleSimpleEvent;
                break;
            case EventType.RecordingStarted:
                GameEvents.OnRecordingStarted -= HandleRecordingStartedEvent;
                break;
            case EventType.RecordingStopped:
                GameEvents.OnRecordingStopped -= HandleSimpleEvent;
                break;
            case EventType.RecordableEventStarted:
                GameEvents.OnRecordableEventStarted -= HandleIdEvent;
                break;
            case EventType.RecordableEventCompleted:
                GameEvents.OnRecordableEventCompleted -= HandleIdEvent;
                break;
            case EventType.RecordableEventInterrupted:
                GameEvents.OnRecordableEventInterrupted -= HandleIdEvent;
                break;
            case EventType.RecordableEventWatched:
                GameEvents.OnRecordableEventWatched -= HandleIdEvent;
                break;
        }
    }

    private void HandleSimpleEvent()
    {
        Unsubscribe();
        Complete();
    }

    private void HandleRecordingStartedEvent(RecordingSession session)
    {
        Unsubscribe();
        Complete();
    }

    private void HandleIdEvent(string id)
    {
        if (!string.IsNullOrEmpty(filterId) && id != filterId) return;
        Unsubscribe();
        Complete();
    }

    //protected override void OnReset()
    //{
    //    Unsubscribe();
    //}
}