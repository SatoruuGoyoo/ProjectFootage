using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class PhoneDialer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI displayText;

    [Header("Buttons")]
    public Button[] numberButtons;
    public Button btnClear;
    public Button btnCall;

    [Header("Config")]
    public string correctCode = "1234";
    public int maxDigits = 4;

    [Header("Phone")]
    public PhoneInteractable phone;

    [Header("On Success")]
    public GameObject objectToActivate;
    public UnityEvent onPuzzleSolved;

    private string currentInput = "";

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
        if (phone != null) phone.PlayMarkNumber();

        if (currentInput == correctCode)
        {
            if (objectToActivate != null)
                objectToActivate.SetActive(false);

            onPuzzleSolved?.Invoke();
        }
        else
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