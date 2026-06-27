using UnityEngine;
using UnityEngine.Events;
using FMODUnity;
using FMOD.Studio;

[RequireComponent(typeof(Collider))]
public class PhoneInteractable : Interactable
{
    [Header("Auto Close")]
    [SerializeField] private Transform _player;
    [SerializeField] private float _maxDistance = 1.5f;

    [Header("FMOD")]
    [SerializeField] private EventReference putUpPhoneReference;
    [SerializeField] private EventReference putDownPhoneReference;
    [SerializeField] private EventReference markNumberReference;
    [SerializeField] private EventReference wrongCodeReference;
    [SerializeField] private EventReference correctCodeReference;
    [SerializeField] private EventReference ringReference;
    [SerializeField] private EventReference callConversationReference;

    [SerializeField] private GameObject _phoneUI;

    [Header("Settings")]
    [SerializeField] private string _openPrompt = "";
    [SerializeField] private string _closePrompt = "";
    [SerializeField] private string _answerPrompt = "";

    [Header("Events")]
    public UnityEvent OnPhoneOpened;
    public UnityEvent OnPhoneClosed;
    public UnityEvent OnCallAnswered;
    public UnityEvent OnConversationEnded;

    private enum PhoneState
    {
        Idle,
        Open,
        WaitingForRing,  // codigo correcto escuchado, telefono cerrado, esperando alejarse
        Ringing,         // sonando, esperando que el jugador conteste
        LockedCorrect,   // escuchando audio de codigo correcto, no puede cerrar HASTA que termine
        LockedCall,      // escuchando conversacion, no puede cerrar
        Done,            // llamada terminada, telefono inutilizable para siempre
    }

    private PhoneState _state = PhoneState.Idle;
    private EventInstance _currentPhoneAudio;

    public override string PromptMessage
    {
        get
        {
            if (_state == PhoneState.Ringing && !_isOpen) return _answerPrompt;
            return _isOpen ? _closePrompt : _openPrompt;
        }
    }

    public override bool CanInteract => _state != PhoneState.Done;
    public override bool BlockMovement => true;
    public bool IsOpen => _isOpen;
    public EventReference MarkNumberReference => markNumberReference;

    private bool _isOpen => _state == PhoneState.Open
                         || _state == PhoneState.LockedCorrect
                         || _state == PhoneState.LockedCall;

    private void Update()
    {
        switch (_state)
        {
            case PhoneState.Open:
                // auto-close por distancia
                if (_player != null && Vector3.Distance(transform.position, _player.position) > _maxDistance)
                    ClosePhone();
                break;

            case PhoneState.WaitingForRing:
                // espera a que el jugador se aleje para empezar a sonar
                if (_player != null && Vector3.Distance(transform.position, _player.position) > _maxDistance)
                    StartRinging();
                break;

            case PhoneState.LockedCorrect:
                // Auto-cierre cuando termina el audio del codigo correcto.
                // El jugador tambien puede interrumpirlo manualmente via Interact().
                if (IsAudioFinished())
                    ClosePhone();
                break;

            case PhoneState.LockedCall:
                if (IsAudioFinished())
                {
                    // Cierra el telefono, abre la puerta, y lo deja inutilizable
                    _phoneUI.SetActive(false);
                    StopCurrentPhoneAudio();
                    if (!putDownPhoneReference.IsNull)
                        RuntimeManager.PlayOneShot(putDownPhoneReference, transform.position);
                    GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
                    if (MouseCursorController.Instance != null) MouseCursorController.Instance.ReleaseCursor();
                    _state = PhoneState.Done;
                    OnPhoneClosed?.Invoke();
                    OnConversationEnded?.Invoke(); // abre la puerta
                }
                break;
        }
    }

    public override void Interact()
    {
        switch (_state)
        {
            case PhoneState.Idle:
                OpenPhone();
                _state = PhoneState.Open;
                break;

            case PhoneState.Open:
                ClosePhone();
                break;

            case PhoneState.LockedCorrect:
                // Permite cerrar en cualquier momento, haya terminado o no el audio.
                // StopCurrentPhoneAudio() se llama dentro de ClosePhone().
                ClosePhone();
                break;

            case PhoneState.Ringing:
                AnswerCall();
                break;

            case PhoneState.LockedCall:
                // no hace nada, bloqueado
                break;

            case PhoneState.Done:
                // ya termino todo, no hace nada
                break;
        }
    }

    public void ForceClose()
    {
        StopCurrentPhoneAudio();
        _state = PhoneState.Idle;
        _phoneUI.SetActive(false);
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.ReleaseCursor();
        GameEvents.InteractPromptDeactivated();
        OnPhoneClosed?.Invoke();
    }

    public void PlayMarkNumber()
    {
        if (markNumberReference.IsNull) return;
        RuntimeManager.PlayOneShot(markNumberReference, transform.position);
    }

    public void PlayWrongCode()
    {
        if (wrongCodeReference.IsNull) return;
        StopCurrentPhoneAudio();
        _currentPhoneAudio = RuntimeManager.CreateInstance(wrongCodeReference);
        _currentPhoneAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _currentPhoneAudio.start();
    }

    public void PlayCorrectCode()
    {
        if (correctCodeReference.IsNull) return;
        StopCurrentPhoneAudio();
        _state = PhoneState.LockedCorrect;
        _currentPhoneAudio = RuntimeManager.CreateInstance(correctCodeReference);
        _currentPhoneAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _currentPhoneAudio.start();
    }

    private void StartRinging()
    {
        _state = PhoneState.Ringing;
        if (ringReference.IsNull) return;
        StopCurrentPhoneAudio();
        _currentPhoneAudio = RuntimeManager.CreateInstance(ringReference);
        _currentPhoneAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _currentPhoneAudio.start();
    }

    private void AnswerCall()
    {
        StopCurrentPhoneAudio(); // corta el ring
        _state = PhoneState.LockedCall;
        OpenPhone();
        OnCallAnswered?.Invoke();

        if (callConversationReference.IsNull) return;
        _currentPhoneAudio = RuntimeManager.CreateInstance(callConversationReference);
        _currentPhoneAudio.set3DAttributes(RuntimeUtils.To3DAttributes(transform.position));
        _currentPhoneAudio.start();
    }

    private void OpenPhone()
    {
        _phoneUI.SetActive(true);
        if (!putUpPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putUpPhoneReference, transform.position);
        GameEvents.PlayerModeChanged(PlayerMode.InteractionMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.RequestCursor();
        GameEvents.InteractPromptActivated(PromptMessage);
        OnPhoneOpened?.Invoke();
    }

    private void ClosePhone()
    {
        if (_state == PhoneState.LockedCorrect)
            _state = PhoneState.WaitingForRing;
        else
            _state = PhoneState.Idle;

        _phoneUI.SetActive(false);
        StopCurrentPhoneAudio();
        if (!putDownPhoneReference.IsNull)
            RuntimeManager.PlayOneShot(putDownPhoneReference, transform.position);
        GameEvents.PlayerModeChanged(PlayerMode.ExplorationMode);
        if (MouseCursorController.Instance != null) MouseCursorController.Instance.ReleaseCursor();
        GameEvents.InteractPromptDeactivated();
        OnPhoneClosed?.Invoke();
    }

    private bool IsAudioFinished()
    {
        if (!_currentPhoneAudio.isValid()) return true;
        _currentPhoneAudio.getPlaybackState(out PLAYBACK_STATE state);
        return state == PLAYBACK_STATE.STOPPED;
    }

    private void StopCurrentPhoneAudio()
    {
        if (_currentPhoneAudio.isValid())
        {
            _currentPhoneAudio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _currentPhoneAudio.release();
        }
    }

    private void OnDestroy()
    {
        StopCurrentPhoneAudio();
    }
}