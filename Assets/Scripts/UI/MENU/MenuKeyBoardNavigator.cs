using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;


public sealed class MenuKeyBoardNavigator : MonoBehaviour
{
    [Header("Navigation Order")]
    [SerializeField] private Button[] menuButtons;

    [Header("Sound")]
    [SerializeField] private FMODUnity.EventReference moveEvent;
    [SerializeField] private bool playSoundOnMove = true;

    private ButtonHoverEffect[] _hoverEffects;
    private int _currentIndex;
    private bool _submitQueued;
    private bool _navigationEnabled;

    private InputAction _moveUpAction;
    private InputAction _moveDownAction;
    private InputAction _submitAction;

    public bool NavigationEnabled => _navigationEnabled;

    private void Awake()
    {
        if (menuButtons == null)
            menuButtons = System.Array.Empty<Button>();

        _hoverEffects = new ButtonHoverEffect[menuButtons.Length];
        for (int i = 0; i < menuButtons.Length; i++)
        {
            if (menuButtons[i] != null)
                _hoverEffects[i] = menuButtons[i].GetComponent<ButtonHoverEffect>();
        }

        _moveUpAction = new InputAction(name: "MenuMoveUp", type: InputActionType.Button, binding: "<Keyboard>/w");
        _moveDownAction = new InputAction(name: "MenuMoveDown", type: InputActionType.Button, binding: "<Keyboard>/s");
        _submitAction = new InputAction(name: "MenuSubmit", type: InputActionType.Button, binding: "<Keyboard>/e");

        _moveUpAction.performed += OnMoveUp;
        _moveDownAction.performed += OnMoveDown;
        _submitAction.performed += OnSubmit;
    }

    private void OnEnable()
    {
        if (_navigationEnabled)
            EnableInput();
    }

    private void OnDisable()
    {
        _submitQueued = false;
        UnhighlightCurrent();
        DisableInput();
    }

    private void OnDestroy()
    {
        _moveUpAction.performed -= OnMoveUp;
        _moveDownAction.performed -= OnMoveDown;
        _submitAction.performed -= OnSubmit;

        _moveUpAction.Dispose();
        _moveDownAction.Dispose();
        _submitAction.Dispose();
    }

    public void SetNavigationEnabled(bool value)
    {
        if (_navigationEnabled == value) return;

        _navigationEnabled = value;
        _submitQueued = false;

        if (!isActiveAndEnabled) return;

        if (value)
        {
            EnableInput();
            _currentIndex = FirstSelectableIndex();
            HighlightCurrent();
        }
        else
        {
            UnhighlightCurrent();
            DisableInput();
        }
    }

    private void EnableInput()
    {
        _moveUpAction.Enable();
        _moveDownAction.Enable();
        _submitAction.Enable();
    }

    private void DisableInput()
    {
        _moveUpAction.Disable();
        _moveDownAction.Disable();
        _submitAction.Disable();
    }

    private void OnMoveUp(InputAction.CallbackContext ctx) => Move(-1);
    private void OnMoveDown(InputAction.CallbackContext ctx) => Move(1);

    // No invoques un onClick (especialmente Application.Quit) dentro del
    // callback nativo del Input System. Se confirma en el siguiente frame.
    private void OnSubmit(InputAction.CallbackContext ctx) => _submitQueued = true;

    private void Update()
    {
        if (!_submitQueued) return;

        _submitQueued = false;

        if (_navigationEnabled)
            Confirm();
    }

    private void Move(int direction)
    {
        if (!_navigationEnabled) return;
        if (menuButtons == null || menuButtons.Length == 0) return;

        UnhighlightCurrent();

        // Salta huecos del array y botones desactivados/no interactuables.
        for (int i = 0; i < menuButtons.Length; i++)
        {
            _currentIndex = (_currentIndex + direction + menuButtons.Length) % menuButtons.Length;
            Button candidate = menuButtons[_currentIndex];
            if (candidate != null && candidate.isActiveAndEnabled && candidate.IsInteractable())
                break;
        }

        HighlightCurrent();

        if (playSoundOnMove && !moveEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(moveEvent);
    }

    private void Confirm()
    {
        if (menuButtons == null || _currentIndex < 0 || _currentIndex >= menuButtons.Length) return;

        Button target = menuButtons[_currentIndex];
        if (target != null && target.IsInteractable())
            target.onClick.Invoke();
    }

    private int FirstSelectableIndex()
    {
        if (menuButtons == null) return 0;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            Button candidate = menuButtons[i];
            if (candidate != null && candidate.isActiveAndEnabled && candidate.IsInteractable())
                return i;
        }

        return 0;
    }

    private void HighlightCurrent()
    {
        if (!_navigationEnabled) return;

        if (_currentIndex >= 0 && _currentIndex < _hoverEffects.Length)
        {
            _hoverEffects[_currentIndex]?.SetHighlighted(true);
            menuButtons[_currentIndex]?.Select();
        }
    }

    private void UnhighlightCurrent()
    {
        if (_hoverEffects == null) return;

        if (_currentIndex >= 0 && _currentIndex < _hoverEffects.Length)
            _hoverEffects[_currentIndex]?.SetHighlighted(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (menuButtons == null || menuButtons.Length == 0)
            Debug.LogWarning($"[{nameof(MenuKeyBoardNavigator)}] No hay botones asignados en '{name}'.", this);
    }
#endif
}