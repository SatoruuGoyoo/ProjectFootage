using System;
using System.Collections.Generic;
using UnityEngine;

public class CamcorderWatchedFeedback : MonoBehaviour
{
    [Serializable]
    private class WatchedFeedbackEntry
    {
        public string eventId;
        public PlayerVoiceLine line;
    }

    [SerializeField] private List<WatchedFeedbackEntry> entries = new List<WatchedFeedbackEntry>();

    private Dictionary<string, PlayerVoiceLine> _lookup;
    private readonly HashSet<string> _alreadyTriggered = new HashSet<string>();
    private string _pendingEventId;
    private bool _menuWasOpen;

    private void Awake()
    {
        _lookup = new Dictionary<string, PlayerVoiceLine>();
        foreach (var entry in entries)
        {
            if (!string.IsNullOrEmpty(entry.eventId))
                _lookup[entry.eventId] = entry.line;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnRecordableEventWatched += HandleWatched;
        GameEvents.OnPlayerModeChanged += HandleModeChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnRecordableEventWatched -= HandleWatched;
        GameEvents.OnPlayerModeChanged -= HandleModeChanged;
    }

    private void HandleWatched(string eventId)
    {
        if (!_menuWasOpen) return;
        if (_alreadyTriggered.Contains(eventId)) return;
        if (!_lookup.ContainsKey(eventId)) return;

        _pendingEventId = eventId;
        _alreadyTriggered.Add(eventId);
    }

    private void HandleModeChanged(PlayerMode newMode)
    {
        bool isMenuOpenNow = newMode == PlayerMode.MenuCameraMode;

        if (isMenuOpenNow)
        {
            _menuWasOpen = true;
            _pendingEventId = null;
            return;
        }

        if (_menuWasOpen && !isMenuOpenNow)
        {
            _menuWasOpen = false;

            if (_pendingEventId != null && _lookup.TryGetValue(_pendingEventId, out var line))
                PlayLine(line);

            _pendingEventId = null;
        }
    }

    private void PlayLine(PlayerVoiceLine line)
    {
        if (PlayerVoicePlayer.Instance == null)
        {
            Debug.LogWarning("[CamcorderWatchedFeedback] No hay PlayerVoicePlayer en escena.");
            return;
        }
        PlayerVoicePlayer.Instance.Play(line);
    }
}