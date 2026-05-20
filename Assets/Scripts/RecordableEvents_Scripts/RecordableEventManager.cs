using UnityEngine;
using System.Collections.Generic;

public class RecordableEventManager : MonoBehaviour
{
    public static RecordableEventManager Instance { get; private set; }

    private readonly List<RecordableEvent> _events = new();
    private readonly HashSet<string> _completedIds = new();

    private void Awake()
    {
        if(Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable() => GameEvents.OnRecordableEventCompleted += HandleCompleted;
    private void OnDisable() => GameEvents.OnRecordableEventCompleted -= HandleCompleted;

    public void Register(RecordableEvent recordableEvent)
    {
        if (!_events.Contains(recordableEvent))
            _events.Add(recordableEvent);
    }

    public void Unregister(RecordableEvent recordableEvent)
    {
        if (_events.Contains(recordableEvent))
            _events.Remove(recordableEvent);
    }

    public bool IsCompleted(string id) => _completedIds.Contains(id);

    public RecordableEvent FindById(string id)
    {
        foreach(var ev in _events)
        {
            if (ev.EventId == id)
            {
                return ev;
            }
        }

        return null;
    }

    private void HandleCompleted(string id)
    {
        _completedIds.Add(id);
        Debug.Log($"[RecordableEventManager] Event completed: {id}");
    }


}
