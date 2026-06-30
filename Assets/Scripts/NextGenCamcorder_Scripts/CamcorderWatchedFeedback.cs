using System.Collections.Generic;
using UnityEngine;

public class CamcorderWatchedFeedback : MonoBehaviour
{
    [SerializeField] private PlayerVoiceLine fearLine;

    private readonly HashSet<string> _alreadyTriggered = new HashSet<string>();
    private bool _watchedSomethingThisSession;
    private bool _menuWasOpen;

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

        _watchedSomethingThisSession = true;
        _alreadyTriggered.Add(eventId);
    }

    private void HandleModeChanged(PlayerMode newMode)
    {
        bool isMenuOpenNow = newMode == PlayerMode.MenuCameraMode;

        if (isMenuOpenNow)
        {
            _menuWasOpen = true;
            _watchedSomethingThisSession = false;
            return;
        }

        if (_menuWasOpen && !isMenuOpenNow)
        {
            _menuWasOpen = false;

            if (_watchedSomethingThisSession)
                PlayFearFeedback();

            _watchedSomethingThisSession = false;
        }
    }

    private void PlayFearFeedback()
    {
        if (PlayerVoicePlayer.Instance == null)
        {
            Debug.LogWarning("[CamcorderWatchedFeedback] No hay PlayerVoicePlayer en escena.");
            return;
        }
        PlayerVoicePlayer.Instance.Play(fearLine);
    }
}