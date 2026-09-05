using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PhoneAudio : MonoBehaviour
{
    [Header("Handset")]
    [SerializeField] private EventReference pickUpReference;
    [SerializeField] private EventReference hangUpReference;

    [Header("Keypad")]
    [SerializeField] private EventReference keyPressReference;
    [SerializeField] private EventReference navigateReference;

    [Header("Code Result")]
    [SerializeField] private EventReference wrongCodeReference;
    [SerializeField] private EventReference correctCodeReference;

    private EventInstance _resultInstance;

    public void PlayPickUp() => PlayOneShot(pickUpReference);
    public void PlayHangUp() => PlayOneShot(hangUpReference);
    public void PlayKeyPress() => PlayOneShot(keyPressReference);
    public void PlayNavigate() => PlayOneShot(navigateReference);

    public void PlayCodeResult(bool correct)
    {
        StopCodeResult();

        EventReference reference = correct ? correctCodeReference : wrongCodeReference;
        if (reference.IsNull) return;

        _resultInstance = RuntimeManager.CreateInstance(reference);
        _resultInstance.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _resultInstance.start();
    }

    public void StopCodeResult()
    {
        if (!_resultInstance.isValid()) return;
        _resultInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _resultInstance.release();
    }

    private void PlayOneShot(EventReference reference)
    {
        if (reference.IsNull) return;
        RuntimeManager.PlayOneShot(reference, transform.position);
    }

    private void OnDestroy() => StopCodeResult();
}