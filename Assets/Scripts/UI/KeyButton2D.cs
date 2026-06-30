using FMODUnity;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.UI;

public class KeyButton2D : MonoBehaviour
{
    public enum CamcorderAction
    {
        OpenCloseMenu,
        NavigateRight,
        NavigateLeft,
        PlayPause,
        Rewind,
        FastForward,
        Discard,
        Stop
    }

    [SerializeField] private CamcorderAction assignedAction;
    [SerializeField] private CamcorderInput input;
    [SerializeField] private Color colorRest = Color.white;
    [SerializeField] private Color colorPressed = new Color(1f, 0.30f, 0.30f, 1f);
    [SerializeField] private float transitionSpeed = 20f;

    [Header("FMOD")]
    [SerializeField] private EventReference pressSound;
    [SerializeField] private EventReference releaseSound;

    private Image _image;
    private Color _currentColor;
    private bool _wasHeld;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _currentColor = colorRest;
    }

    private void OnEnable()
    {
        if (input != null) return;
        input = GetComponentInParent<CamcorderInput>();
    }

    private void Update()
    {
        if (input == null) return;

        bool held = assignedAction switch
        {
            CamcorderAction.OpenCloseMenu => input.OpenCloseMenuHeld,
            CamcorderAction.NavigateRight => input.NavigateRightHeld,
            CamcorderAction.NavigateLeft => input.NavigateLeftHeld,
            CamcorderAction.PlayPause => input.PlayPauseRecordingHeld,
            CamcorderAction.Rewind => input.RewindRecording,
            CamcorderAction.FastForward => input.FastForwardRecording,
            CamcorderAction.Discard => input.DiscardRecordingHeld,
            CamcorderAction.Stop => input.StopRecordingHeld,
            _ => false
        };

        if (held && !_wasHeld) PlaySound(pressSound);
        else if (!held && _wasHeld) PlaySound(releaseSound);

        _wasHeld = held;

        Color target = held ? colorPressed : colorRest;
        _currentColor = Color.Lerp(_currentColor, target, Time.deltaTime * transitionSpeed);
        _image.color = _currentColor;
    }

    private void PlaySound(EventReference eventRef)
    {
        if (eventRef.IsNull) return;
        EventInstance instance = RuntimeManager.CreateInstance(eventRef);
        instance.start();
        instance.release();
    }
}