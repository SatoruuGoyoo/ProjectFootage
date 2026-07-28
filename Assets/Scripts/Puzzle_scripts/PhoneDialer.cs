using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PhoneDialer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI displayText;

    [Header("Buttons")]
    public Button[] numberButtons;
    public Button btnClear;
    public Button btnCall;

    [Header("Navigation Grid")]
    [SerializeField] private RectTransform buttonGrid;
    [SerializeField] private int columns = 3;

    [Header("Config")]
    public string correctCode = "1234";
    public int maxDigits = 4;

    [Header("Selection Highlight")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.3f);

    [Header("Phone")]
    public PhoneInteractable phone;

    private string currentInput = "";
    private Button[] _navGrid;
    private int _currentIndex;
    private int _lastHighlighted = -1;
    private InputAction _navigateAction;
    private InputAction _submitAction;
    private bool _navXNeutral = true;
    private bool _navYNeutral = true;
    private bool _suppressSubmitThisFrame;
    private bool _wasPhoneActive;

    void Start()
    {
        foreach (Button btn in numberButtons)
        {
            string key = btn.gameObject.name.Replace("Btn_", "");
            btn.onClick.AddListener(() => OnKeyPressed(key));
        }
        btnClear.onClick.AddListener(OnClear);
        btnCall.onClick.AddListener(OnCall);
        UpdateDisplay();

        _navGrid = buttonGrid.GetComponentsInChildren<Button>();
        _navigateAction = PlayerInput.Actions.UI.Navigate;
        _submitAction = PlayerInput.Actions.UI.Submit;
    }

    private void Update()
    {
        bool phoneActive = phone != null && phone.IsActive;

        if (phoneActive && !_wasPhoneActive)
        {
            _currentIndex = 0;
            _lastHighlighted = -1;
            _navXNeutral = true;
            _navYNeutral = true;
            _suppressSubmitThisFrame = true;
        }
        _wasPhoneActive = phoneActive;

        if (!phoneActive) return;
        if (_navGrid == null || _navGrid.Length == 0) return;

        HighlightCurrent();

        Vector2 nav = _navigateAction.ReadValue<Vector2>();

        if (_navXNeutral)
        {
            if (nav.x > 0.5f) MoveHorizontal(1);
            else if (nav.x < -0.5f) MoveHorizontal(-1);

            if (Mathf.Abs(nav.x) > 0.5f) _navXNeutral = false;
        }
        else if (Mathf.Abs(nav.x) < 0.1f)
        {
            _navXNeutral = true;
        }

        if (_navYNeutral)
        {
            if (nav.y > 0.5f) MoveVertical(-1);
            else if (nav.y < -0.5f) MoveVertical(1);

            if (Mathf.Abs(nav.y) > 0.5f) _navYNeutral = false;
        }
        else if (Mathf.Abs(nav.y) < 0.1f)
        {
            _navYNeutral = true;
        }

        if (_suppressSubmitThisFrame)
        {
            _suppressSubmitThisFrame = false;
            return;
        }

        if (_submitAction.WasPressedThisFrame())
            _navGrid[_currentIndex].onClick.Invoke();
    }

    private void MoveHorizontal(int dir)
    {
        int col = _currentIndex % columns;
        if (dir > 0 && col >= columns - 1) return;
        if (dir < 0 && col <= 0) return;

        int next = _currentIndex + dir;
        if (next < 0 || next >= _navGrid.Length) return;
        _currentIndex = next;
    }

    private void MoveVertical(int dir)
    {
        int next = _currentIndex + dir * columns;
        if (next < 0 || next >= _navGrid.Length) return;
        _currentIndex = next;
    }

    private void HighlightCurrent()
    {
        if (_currentIndex == _lastHighlighted) return;
        bool isInitialHighlight = _lastHighlighted < 0;
        _lastHighlighted = _currentIndex;

        for (int i = 0; i < _navGrid.Length; i++)
        {
            var graphic = _navGrid[i].targetGraphic;
            if (graphic == null) continue;
            graphic.color = (i == _currentIndex) ? highlightColor : normalColor;
        }

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(_navGrid[_currentIndex].gameObject);

        if (!isInitialHighlight && phone != null) phone.PlayNavigate();
    }

    void OnKeyPressed(string key)
    {
        if (currentInput.Length >= maxDigits) return;
        currentInput += key;
        if (phone != null) phone.PlayMarkNumber();
        UpdateDisplay();
    }

    void OnClear()
    {
        if (currentInput.Length > 0)
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        if (phone != null) phone.PlayMarkNumber();
        UpdateDisplay();
    }

    void OnCall()
    {
        bool correct = currentInput == correctCode;
        if (phone != null) phone.SubmitCode(correct);
        if (!correct)
        {
            currentInput = "";
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        displayText.text = currentInput;
    }
}