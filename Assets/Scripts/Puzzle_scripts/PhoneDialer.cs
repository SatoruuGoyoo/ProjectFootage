using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PhoneDialer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PhoneInteractable phone;
    [SerializeField] private PhoneAudio phoneAudio;
    [SerializeField] private TextMeshProUGUI displayText;

    [Header("Navigation Grid")]
    [SerializeField] private RectTransform buttonGrid;
    [SerializeField] private int columns = 3;

    [Header("Special Keys")]
    [SerializeField] private Button clearButton;
    [SerializeField] private Button callButton;

    [Header("Number Keys")]
    [SerializeField] private string keyNamePrefix = "Btn_";

    [Header("Config")]
    [SerializeField] private int maxDigits = 4;

    [Header("Selection Highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f);

    private Button[] _keys;
    private string _input = "";
    private int _index;
    private bool _active;
    private bool _suppressSubmitThisFrame;
    private UINavigationInput _navigation;
    private InputAction _submitAction;

    private void Start()
    {
        _keys = buttonGrid != null ? buttonGrid.GetComponentsInChildren<Button>(true) : new Button[0];
        _navigation = new UINavigationInput(PlayerInput.Actions.UI.Navigate);
        _submitAction = PlayerInput.Actions.UI.Submit;
        UpdateDisplay();
    }

    public void Begin()
    {
        _active = true;
        _index = 0;
        _suppressSubmitThisFrame = true;
        _navigation?.Reset();
        ClearInput();
        RefreshHighlight();
    }

    public void End()
    {
        _active = false;
    }

    public void ClearInput()
    {
        _input = "";
        UpdateDisplay();
    }

    private void Update()
    {
        if (!_active || _keys == null || _keys.Length == 0) return;

        Vector2Int step = _navigation.Read();
        if (step.x != 0) Move(step.x);
        if (step.y != 0) Move(-step.y * columns);

        if (_suppressSubmitThisFrame)
        {
            _suppressSubmitThisFrame = false;
            return;
        }

        if (_submitAction.WasPressedThisFrame()) Activate(_keys[_index]);
    }

    private void Move(int offset)
    {
        int next = _index + offset;

        if (Mathf.Abs(offset) == 1)
        {
            int column = _index % columns;
            if (offset > 0 && column >= columns - 1) return;
            if (offset < 0 && column <= 0) return;
        }

        if (next < 0 || next >= _keys.Length) return;

        _index = next;
        phoneAudio?.PlayNavigate();
        RefreshHighlight();
    }

    private void RefreshHighlight()
    {
        for (int i = 0; i < _keys.Length; i++)
        {
            var graphic = _keys[i].targetGraphic;
            if (graphic == null) continue;
            graphic.color = i == _index ? highlightColor : normalColor;
        }
    }

    private void Activate(Button key)
    {
        if (key == clearButton) { Backspace(); return; }
        if (key == callButton) { Submit(); return; }

        AppendDigit(KeyValueOf(key));
    }

    private string KeyValueOf(Button key) =>
        string.IsNullOrEmpty(keyNamePrefix) ? key.name : key.name.Replace(keyNamePrefix, "");

    private void AppendDigit(string digit)
    {
        if (string.IsNullOrEmpty(digit)) return;
        if (_input.Length >= maxDigits) return;

        _input += digit;
        phoneAudio?.PlayKeyPress();
        UpdateDisplay();
    }

    private void Backspace()
    {
        if (_input.Length > 0) _input = _input.Substring(0, _input.Length - 1);
        phoneAudio?.PlayKeyPress();
        UpdateDisplay();
    }

    private void Submit()
    {
        if (phone == null) return;
        phone.SubmitCode(_input);
    }

    private void UpdateDisplay()
    {
        if (displayText != null) displayText.text = _input;
    }
}