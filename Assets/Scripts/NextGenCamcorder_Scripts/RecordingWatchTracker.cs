using UnityEngine;

[RequireComponent(typeof(PlaybackClock))]
public class RecordingWatchTracker : MonoBehaviour
{
    private PlaybackClock clock;

    private void Awake() => clock = GetComponent<PlaybackClock>();

    private void OnEnable() => clock.OnComplete += HandleComplete;
    private void OnDisable() => clock.OnComplete -= HandleComplete;

    private void HandleComplete()
    {
        var session = clock.CurrentSession;
        if (session == null) return;

        foreach (var id in session.EventIds)
            GameEvents.RecordableEventWatched(id);
    }
}