using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Collider))]
public class PhoneInteractable : Interactable
{
    [Header("Broken State")]
    [SerializeField] private bool isBroken = false;
    [TextArea(3, 6)]
    [SerializeField] private string brokenMessage = "El telefono no funciona";
    [SerializeField] private UIPositioner.ScreenPosition brokenMessagePosition = UIPositioner.ScreenPosition.LowerCenter;
    [SerializeField] private SubtitleBlock brokenSubtitles;
    [SerializeField] private float brokenSubtitleDuration = 3f;

    [Header("FMOD")]
    [SerializeField] private EventReference putUpPhoneReference;
    [SerializeField] private EventReference putDownPhoneReference;
    [SerializeField] private EventReference markNumberReference;
    [SerializeField] private EventReference wrongCodeReference;
    [SerializeField] private EventReference correctCodeReference;
    [SerializeField] private EventReference navigateSound;

    [SerializeField] private GameObject _phoneUI;

    [Header("Prompts")]
    [SerializeField] private string _openPrompt = "";
    [SerializeField] private string _closePrompt = "";
    [SerializeField] private string _answerPrompt = "";

    [Header("Events")]
    public UnityEvent OnPickedUp;
    public UnityEvent OnHungUp;
    public UnityEvent OnCodeCorrect;
    public UnityEvent OnCodeWrong;
    public UnityEvent OnCallAnswered;
    public UnityEvent OnBrokenInteract;

    private enum PhoneState
    {
        Idle,
        Open,
        Ringing,
        Answered,
        Done,
    }

    private PhoneState _state = PhoneState.Idle;
    private InputAction _cancelAction;
    private Coroutine _brokenHideRoutine;
    private EventInstance _codeAudio;

    public override string PromptMessage
    {
        get
        {
            if (_state == PhoneState.Ringing) return _answerPrompt;
            return _state == PhoneState.Open ? _closePrompt : _openPrompt;
        }
    }

    public override bool CanInteract => _state != PhoneState.Done;
    public override bool BlockMovement => true;
    public override bool IsActive => _state == PhoneState.Open || _state == PhoneState.Answered;

    private void Start()
    {
        _cancelAction = PlayerInput.Actions.UI.Cancel;
    }

    private void Update()
    {
        if (_cancelAction.WasPressedThisFrame() && _state == PhoneState.Open)
            HangUp();
    }

    public override void Interact()
    {
        switch (_state)
        {
            case PhoneState.Idle:
                if (isBroken)
                {
                    ShowBrokenMessage();
                    OnBrokenInteract?.Invoke();
                    return;
                }
                OpenPhone();
                break;

            case PhoneState.Ringing:
                AnswerCall();
                break;
        }
    }

    public void SetRinging(bool ringing)
    {
        if (_state == PhoneState.Done) return;
        _state = ringing ? PhoneState.Ringing : PhoneState.Idle;
    }

    public void SubmitCode(bool correct)
    {
        StopCodeAudio();

        if (correct)
        {
            if (!correctCodeReference.IsNull)
            {
                _codeAudio = RuntimeManager.CreateInstance(correctCodeReference);
                _codeAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                _codeAudio.start();
            }
            OnCodeCorrect?.Invoke();
            CloseAndRelease(playHangUpSound: false);
        }
        else
        {
            if (!wrongCodeReference.IsNull)
            {
                _codeAudio = RuntimeManager.CreateInstance(wrongCodeReference);
                _codeAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
                _codeAudio.start();
            }
            OnCodeWrong?.Invoke();
        }
    }

    public void MarkAnsweredComplete()
    {
        StopCodeAudio();
        ExitInteractionMode();
        _state = PhoneState.Done;
        OnHungUp?.Invoke();
    }

    public void ForceClose()
    {
        StopCodeAudio();
        CloseAndRelease();
        _state = PhoneState.Idle;
        OnHungUp?.Invoke();
    }

    public void PlayMarkNumber()
    {
        if (markNumberReference.IsNull) return;
        RuntimeManager.PlayOneShot(markNumberReference, transform.position);
    }

    public void PlayNavigate()
    {
        if (navigateSound.IsNull) return;
        RuntimeManager.PlayOneShot(navigateSound, transform.position);
    }

    private void AnswerCall()
    {
        _state = PhoneState.Answered;
        if (_phoneUI != null) _phoneUI.SetActive(false);
        if (!putUpPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putUpPhoneReference, transform.position);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
        GameEvents.InteractPromptDeactivated();
        OnCallAnswered?.Invoke();
    }

    private void OpenPhone()
    {
        _state = PhoneState.Open;
        if (_phoneUI != null) _phoneUI.SetActive(true);
        if (!putUpPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putUpPhoneReference, transform.position);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.RequestCursor();
        GameEvents.InteractPromptActivated(PromptMessage, ActiveIcon);
        OnPickedUp?.Invoke();
    }

    private void HangUp()
    {
        StopCodeAudio();
        CloseAndRelease();
        _state = PhoneState.Idle;
        OnHungUp?.Invoke();
    }

    private void CloseAndRelease(bool playHangUpSound = true)
    {
        if (_phoneUI != null) _phoneUI.SetActive(false);
        if (playHangUpSound && !putDownPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putDownPhoneReference, transform.position);
        GameEvents.InteractPromptDeactivated();
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.ReleaseCursor();
    }

    private void ExitInteractionMode()
    {
        GameEvents.InteractPromptDeactivated();
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.ReleaseCursor();
    }

    private void StopCodeAudio()
    {
        if (_codeAudio.isValid())
        {
            _codeAudio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _codeAudio.release();
        }
    }

    private void ShowBrokenMessage()
    {
        if (brokenSubtitles == null) return;
        brokenSubtitles.Show(brokenMessage, brokenMessagePosition);
        if (_brokenHideRoutine != null) StopCoroutine(_brokenHideRoutine);
        _brokenHideRoutine = StartCoroutine(HideBrokenAfterDelay());
    }

    private System.Collections.IEnumerator HideBrokenAfterDelay()
    {
        yield return new WaitForSeconds(brokenSubtitleDuration);
        if (brokenSubtitles != null) brokenSubtitles.Hide();
        _brokenHideRoutine = null;
    }

    private void OnDestroy()
    {
        StopCodeAudio();
    }
}