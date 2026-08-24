using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

[RequireComponent(typeof(Collider))]
public class PhoneInteractable : Interactable
{
    [Header("Puzzle")]
    [SerializeField] private string correctCode = "1234";

    [Header("Broken State")]
    [SerializeField] private bool isBroken = false;
    [TextArea(3, 6)]
    [SerializeField] private string brokenMessage = "El telefono no funciona";
    [SerializeField] private UIPositioner.ScreenPosition brokenMessagePosition = UIPositioner.ScreenPosition.LowerCenter;
    [SerializeField] private SubtitleBlock brokenSubtitles;
    [SerializeField] private float brokenSubtitleDuration = 3f;

    [Header("References")]
    [SerializeField] private PhoneAudio phoneAudio;
    [SerializeField] private PhoneDialer dialer;
    [FormerlySerializedAs("_phoneUI")]
    [SerializeField] private GameObject phoneUI;

    [Header("Prompts")]
    [FormerlySerializedAs("_openPrompt")]
    [SerializeField] private string openPrompt = "";
    [FormerlySerializedAs("_closePrompt")]
    [SerializeField] private string closePrompt = "";
    [FormerlySerializedAs("_answerPrompt")]
    [SerializeField] private string answerPrompt = "";

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
    private Coroutine _brokenHideRoutine;

    public override string PromptMessage => _state switch
    {
        PhoneState.Idle => openPrompt,
        PhoneState.Ringing => answerPrompt,
        PhoneState.Open => closePrompt,
        _ => "",
    };

    public override bool CanInteract => _state == PhoneState.Idle || _state == PhoneState.Ringing;
    public override bool IsActive => _state == PhoneState.Open || _state == PhoneState.Answered;
    public override bool BlockMovement => true;

    private void Awake()
    {
        if (phoneAudio == null) phoneAudio = GetComponent<PhoneAudio>();
        SetUIActive(false);
    }

    public override void Interact()
    {
        switch (_state)
        {
            case PhoneState.Idle:
                if (isBroken) ReportBroken();
                else OpenPhone();
                break;

            case PhoneState.Ringing:
                AnswerCall();
                break;
        }
    }

    public override void Cancel()
    {
        if (_state != PhoneState.Open) return;
        HangUp();
    }

    public void SetRinging(bool ringing)
    {
        if (_state == PhoneState.Done) return;
        _state = ringing ? PhoneState.Ringing : PhoneState.Idle;
    }

    public void SubmitCode(string code)
    {
        if (isBroken || _state != PhoneState.Open) return;

        bool correct = code == correctCode;
        phoneAudio?.PlayCodeResult(correct);

        if (correct)
        {
            OnCodeCorrect?.Invoke();
            _state = PhoneState.Idle;
            CloseAndRelease(playHangUpSound: false);
            return;
        }

        OnCodeWrong?.Invoke();
        dialer?.ClearInput();
    }

    public void MarkAnsweredComplete()
    {
        phoneAudio?.StopCodeResult();
        _state = PhoneState.Done;
        ExitInteractionMode();
        OnHungUp?.Invoke();
    }

    public void ForceClose()
    {
        phoneAudio?.StopCodeResult();
        _state = PhoneState.Idle;
        CloseAndRelease();
        OnHungUp?.Invoke();
    }

    private void OpenPhone()
    {
        _state = PhoneState.Open;
        SetUIActive(true);
        phoneAudio?.PlayPickUp();
        EnterInteractionMode();
        OnPickedUp?.Invoke();
    }

    private void AnswerCall()
    {
        _state = PhoneState.Answered;
        SetUIActive(false);
        phoneAudio?.PlayPickUp();
        EnterInteractionMode();
        OnCallAnswered?.Invoke();
    }

    private void HangUp()
    {
        phoneAudio?.StopCodeResult();
        _state = PhoneState.Idle;
        CloseAndRelease();
        OnHungUp?.Invoke();
    }

    private void CloseAndRelease(bool playHangUpSound = true)
    {
        SetUIActive(false);
        if (playHangUpSound) phoneAudio?.PlayHangUp();
        ExitInteractionMode();
    }

    private void SetUIActive(bool active)
    {
        if (phoneUI != null) phoneUI.SetActive(active);

        if (dialer == null) return;
        if (active) dialer.Begin();
        else dialer.End();
    }

    private void ReportBroken()
    {
        OnBrokenInteract?.Invoke();

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
}