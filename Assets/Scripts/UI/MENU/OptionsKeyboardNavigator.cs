using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class OptionsKeyboardNavigator : MonoBehaviour
{
    [Header("Navigation Order")]
    [SerializeField] private Slider[] sliders;
    [SerializeField] private Button backButton;

    [Header("Editing")]
    [SerializeField] private float valueStep = 0.5f;

    [Header("Sound")]
    [SerializeField] private FMODUnity.EventReference moveEvent;
    [SerializeField] private bool playSoundOnMove = true;

    private ButtonHoverEffect _backHover;
    private SliderHoverEffect[] _sliderHovers;
    private int _currentIndex;
    private bool _submitQueued;

    private InputAction _moveUpAction;
    private InputAction _moveDownAction;
    private InputAction _adjustAction;
    private InputAction _submitAction;
    private InputAction _cancelAction;

    private int TotalItems => sliders.Length + (backButton != null ? 1 : 0);
    private bool OnBackButton => backButton != null && _currentIndex == sliders.Length;

    private void Awake()
    {
        if (sliders == null)
            sliders = System.Array.Empty<Slider>();

        _sliderHovers = new SliderHoverEffect[sliders.Length];
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i] != null)
                _sliderHovers[i] = sliders[i].GetComponent<SliderHoverEffect>();
        }

        if (backButton != null)
            _backHover = backButton.GetComponent<ButtonHoverEffect>();

        _moveUpAction = new InputAction(name: "OptionsMoveUp", type: InputActionType.Button, binding: "<Keyboard>/w");
        _moveDownAction = new InputAction(name: "OptionsMoveDown", type: InputActionType.Button, binding: "<Keyboard>/s");
        _adjustAction = new InputAction(name: "OptionsAdjust", type: InputActionType.Value, expectedControlType: "Axis");
        _adjustAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        _submitAction = new InputAction(name: "OptionsSubmit", type: InputActionType.Button, binding: "<Keyboard>/e");
        _cancelAction = new InputAction(name: "OptionsCancel", type: InputActionType.Button, binding: "<Keyboard>/escape");

        _moveUpAction.performed += OnMoveUp;
        _moveDownAction.performed += OnMoveDown;
        _submitAction.performed += OnSubmit;
    }

    private void OnEnable()
    {
        _moveUpAction.Enable();
        _moveDownAction.Enable();
        _adjustAction.Enable();
        _submitAction.Enable();
        _cancelAction.Enable();

        _currentIndex = 0;
        HighlightCurrent();
    }

    private void OnDisable()
    {
        _submitQueued = false;
        UnhighlightCurrent();

        _moveUpAction.Disable();
        _moveDownAction.Disable();
        _adjustAction.Disable();
        _submitAction.Disable();
        _cancelAction.Disable();
    }

    private void OnDestroy()
    {
        _moveUpAction.performed -= OnMoveUp;
        _moveDownAction.performed -= OnMoveDown;
        _submitAction.performed -= OnSubmit;

        _moveUpAction.Dispose();
        _moveDownAction.Dispose();
        _adjustAction.Dispose();
        _submitAction.Dispose();
        _cancelAction.Dispose();
    }

    private void OnMoveUp(InputAction.CallbackContext ctx) => Move(-1);
    private void OnMoveDown(InputAction.CallbackContext ctx) => Move(1);
    private void OnSubmit(InputAction.CallbackContext ctx) => _submitQueued = true;

    private void Update()
    {
        if (!OnBackButton && _currentIndex >= 0 && _currentIndex < sliders.Length)
        {
            float axis = _adjustAction.ReadValue<float>();
            if (Mathf.Abs(axis) > 0.1f)
            {
                Slider s = sliders[_currentIndex];
                s.value = Mathf.Clamp01(s.value + axis * valueStep * Time.unscaledDeltaTime);
            }
        }

        if (_submitQueued)
        {
            _submitQueued = false;
            Confirm();
        }

        if (_cancelAction.WasPressedThisFrame())
        {
            if (backButton != null) backButton.onClick.Invoke();
        }
    }

    private void Move(int direction)
    {
        if (TotalItems == 0) return;

        UnhighlightCurrent();
        _currentIndex = (_currentIndex + direction + TotalItems) % TotalItems;
        HighlightCurrent();

        if (playSoundOnMove && !moveEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(moveEvent);
    }

    private void Confirm()
    {
        if (OnBackButton)
        {
            if (backButton.interactable) backButton.onClick.Invoke();
        }
    }

    private void HighlightCurrent()
    {
        if (OnBackButton)
        {
            _backHover?.SetHighlighted(true);
            backButton?.Select();
        }
        else if (_currentIndex >= 0 && _currentIndex < sliders.Length)
        {
            if (_currentIndex < _sliderHovers.Length)
                _sliderHovers[_currentIndex]?.SetHighlighted(true);
            sliders[_currentIndex]?.Select();
        }
    }

    private void UnhighlightCurrent()
    {
        if (OnBackButton)
        {
            _backHover?.SetHighlighted(false);
        }
        else if (_currentIndex >= 0 && _currentIndex < _sliderHovers.Length)
        {
            _sliderHovers[_currentIndex]?.SetHighlighted(false);
        }
    }
}