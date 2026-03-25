using System;
using UnityEngine;

public class CamcorderMenuController : MonoBehaviour
{
    private CamcorderStorage storage;
    private CamcorderPlayback playback;
    private CamcorderInput input;

    private bool IsMenuOpen = false;

    private int currentRecordingIndex = 0;

    private void Awake()
    {
        storage = GetComponent<CamcorderStorage>();
        playback = GetComponent<CamcorderPlayback>();
        input = GetComponent<CamcorderInput>();
    }

    private void Update()
    {   
        ToggleMenu();

        if (IsMenuOpen)
            HandleNavigation();
    }

    private void OpenMenu()
    {
        if (!IsMenuOpen)
        {
            IsMenuOpen = true;
            GameEvents.PlayerModeChanged(PlayerMode.MenuCameraMode);
        }
    }

    private void CloseMenu()
    {
        if (IsMenuOpen)
        {
            IsMenuOpen = false;
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
        if (input.NavigateMenu > 0.1f)
        {
            currentRecordingIndex++;
        }
        else if (input.NavigateMenu < -0.1f)
        {
            currentRecordingIndex--;
        }

        currentRecordingIndex = Mathf.Clamp(currentRecordingIndex, 0, storage.GetAllRecordings().Count - 1);
    }
}
