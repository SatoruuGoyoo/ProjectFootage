using System.Collections.Generic;
using UnityEngine;

public class RecordableEventManager : MonoBehaviour
{
    public static RecordableEventManager Instance { get; private set; }

    private readonly List<RecordableEvent> events = new();
    private readonly HashSet<string> completedIds = new();
    private RecordingSession activeSession;
    private RecordingSession lastSession;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnEnable()
    {
        GameEvents.OnRecordableEventCompleted += HandleCompleted;
        GameEvents.OnRecordableEventInterrupted += HandleInterrupted;
        GameEvents.OnRecordingStarted += HandleRecordingStarted;
        GameEvents.OnRecordingStopped += HandleRecordingStopped;
    }

    private void OnDisable()
    {
        GameEvents.OnRecordableEventCompleted -= HandleCompleted;
        GameEvents.OnRecordableEventInterrupted -= HandleInterrupted;
        GameEvents.OnRecordingStarted -= HandleRecordingStarted;
        GameEvents.OnRecordingStopped -= HandleRecordingStopped;
    }

    public void Register(RecordableEvent ev)
    {
        if (!events.Contains(ev)) events.Add(ev);
    }

    public void Unregister(RecordableEvent ev) => events.Remove(ev);

    public bool IsCompleted(string id) => completedIds.Contains(id);

    public RecordableEvent FindById(string id)
    {
        foreach (var e in events)
            if (e.EventId == id) return e;
        return null;
    }

    private void HandleRecordingStarted(RecordingSession session)
    {
        activeSession = session;
        lastSession = session;
    }

    private void HandleRecordingStopped() => activeSession = null;

    private void HandleCompleted(string id)
    {
        completedIds.Add(id);
        if (activeSession != null) activeSession.RegisterEvent(id);
    }

    private void HandleInterrupted(string id)
    {
        var target = activeSession != null ? activeSession : lastSession;
        Debug.Log($"[Manager] interrupted '{id}' | target null? {target == null}");
        if (target != null) target.MarkAsCorrupted();
    }
}