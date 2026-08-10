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

    private InputAction _moveUpAction;
    private InputAction _moveDownAction;
    private InputAction _submitAction;

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
        _moveUpAction.Enable();
        _moveDownAction.Enable();
        _submitAction.Enable();

        _currentIndex = 0;
        HighlightCurrent();
    }

    private void OnDisable()
    {
        _submitQueued = false;
        UnhighlightCurrent();

        _moveUpAction.Disable();
        _moveDownAction.Disable();
        _submitAction.Disable();
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

    private void OnMoveUp(InputAction.CallbackContext ctx) => Move(-1);
    private void OnMoveDown(InputAction.CallbackContext ctx) => Move(1);

    // No invoques un onClick (especialmente Application.Quit) dentro del
    // callback nativo del Input System. Se confirma en el siguiente frame.
    private void OnSubmit(InputAction.CallbackContext ctx) => _submitQueued = true;

    private void Update()
    {
        if (!_submitQueued) return;

        _submitQueued = false;
        Confirm();
    }

    private void Move(int direction)
    {
        if (menuButtons == null || menuButtons.Length == 0) return;

        UnhighlightCurrent();

        // Salta huecos del array y botones desactivados/no interactuables.
        for (int i = 0; i < menuButtons.Length; i++)
        {
            _currentIndex = (_currentIndex + direction + menuButtons.Length) % menuButtons.Length;
            Button candidate = menuButtons[_currentIndex];
            if (candidate != null && candidate.isActiveAndEnabled && candidate.interactable)
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
        if (target != null && target.interactable)
            target.onClick.Invoke();
    }

    private void HighlightCurrent()
    {
        if (_currentIndex >= 0 && _currentIndex < _hoverEffects.Length)
        {
            _hoverEffects[_currentIndex]?.SetHighlighted(true);
            menuButtons[_currentIndex]?.Select();
        }
    }

    private void UnhighlightCurrent()
    {
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