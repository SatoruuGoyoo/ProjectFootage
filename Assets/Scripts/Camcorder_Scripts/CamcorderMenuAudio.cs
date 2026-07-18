using FMODUnity;
using UnityEngine;

public class CamcorderMenuAudio : MonoBehaviour
{
    [SerializeField] private EventReference navigateSound;
    [SerializeField] private EventReference navigateBlockedSound;
    [SerializeField] private EventReference playPauseSound;
    [SerializeField] private EventReference rffSound;
    [SerializeField] private EventReference stopDiscardSound;

    public void PlayNavigate() => RuntimeManager.PlayOneShot(navigateSound);
    public void PlayNavigateBlocked() => RuntimeManager.PlayOneShot(navigateBlockedSound);
    public void PlayPlayPause() => RuntimeManager.PlayOneShot(playPauseSound);
    public void PlayRFF() => RuntimeManager.PlayOneShot(rffSound);
    public void PlayStopDiscard() => RuntimeManager.PlayOneShot(stopDiscardSound);
}