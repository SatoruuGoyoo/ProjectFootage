using System;
using System.Collections.Generic;
using UnityEngine;

public class RecordableEventFeedbackManager : MonoBehaviour
{
    [Serializable]
    private class EventFeedbackEntry
    {
        public string eventId;
        public PlayerVoiceLine completedLine;
        public PlayerVoiceLine interruptedLine;
    }

    [SerializeField] private List<EventFeedbackEntry> entries = new List<EventFeedbackEntry>();

    private Dictionary<string, EventFeedbackEntry> _lookup;

    private void Awake()
    {
        _lookup = new Dictionary<string, EventFeedbackEntry>();
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry.eventId)) continue;
            _lookup[entry.eventId] = entry;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnRecordableEventCompleted += HandleCompleted;
        GameEvents.OnRecordableEventInterrupted += HandleInterrupted;
    }

    private void OnDisable()
    {
        GameEvents.OnRecordableEventCompleted -= HandleCompleted;
        GameEvents.OnRecordableEventInterrupted -= HandleInterrupted;
    }

    private void HandleCompleted(string eventId)
    {
        if (!_lookup.TryGetValue(eventId, out var entry)) return;
        if (entry.completedLine == null) return;
        PlayLine(entry.completedLine);
    }

    private void HandleInterrupted(string eventId)
    {
        if (!_lookup.TryGetValue(eventId, out var entry)) return;
        if (entry.interruptedLine == null) return;
        PlayLine(entry.interruptedLine);
    }

    private void PlayLine(PlayerVoiceLine line)
    {
        if (PlayerVoicePlayer.Instance == null)
        {
            Debug.LogWarning("[RecordableEventFeedbackManager] No hay PlayerVoicePlayer en escena.");
            return;
        }
        PlayerVoicePlayer.Instance.Play(line);
    }
}