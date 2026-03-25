using System;
using UnityEngine;

public class CamcorderMenuController : MonoBehaviour
{
    private CamcorderStorage storage;
    private CamcorderPlayback playback;
    private CamcorderInput input;
    private CamcorderMenuUI ui;

    public GameObject camcorderMenuPanel;

    private bool IsMenuOpen = false;

    private int currentRecordingIndex = 0;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
        playback = GetComponent<CamcorderPlayback>();
        input = GetComponent<CamcorderInput>();
        ui = GetComponent<CamcorderMenuUI>();
    }

    private void Update()
    {   
        ToggleMenu();

        if (IsMenuOpen)
        {
            HandleNavigation();
            HandlePlayback();
        }
            
    }

    private void OpenMenu()
    {
        if (!IsMenuOpen)
        {
            IsMenuOpen = true;
            camcorderMenuPanel.SetActive(true);
            ui.UpdateUI(currentRecordingIndex);
            GameEvents.PlayerModeChanged(PlayerMode.MenuCameraMode);
        }
    }

    private void CloseMenu()
    {
        if (IsMenuOpen)
        {
            IsMenuOpen = false;
            currentRecordingIndex = 0;
            camcorderMenuPanel.SetActive(false);
            GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        }
    }

    private void ToggleMenu()
    {
        if (input.OpenCloseMenu)
        {
            if (IsMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }

        }
    }
    private void HandleNavigation()
    {
        if (storage.GetAllRecordings().Count == 0) return;

        if (input.NavigateRight)
            currentRecordingIndex++;
        else if (input.NavigateLeft)
            currentRecordingIndex--;

        currentRecordingIndex = Mathf.Clamp(currentRecordingIndex, 0, storage.GetAllRecordings().Count - 1);
        ui.UpdateUI(currentRecordingIndex);
    }

    private void HandlePlayback()
    {
        if (!input.PlayPauseRecording) return;
        if (storage.GetAllRecordings().Count == 0) return;

        if (playback.IsPlaying)
            playback.PausePlayback();
        else if (playback.HasRecording && !playback.IsFinished)
             playback.ResumePlayback();
        else
            playback.PlayRecording(storage.GetAllRecordings()[currentRecordingIndex]);
    }
}
