using UnityEngine;
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

    [Header("Config")]
    public string correctCode = "1234";
    public int maxDigits = 4;

    [Header("On Success")]
    public GameObject objectToActivate;

    private string currentInput = "";

    void Start()
    {
        foreach (Button btn in numberButtons)
        {
            // Lee su propio nombre: "Btn_1" → "1", "Btn_*" → "*", etc.
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
        UpdateDisplay();
    }

    void OnClear()
    {
        if (currentInput.Length > 0)
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        UpdateDisplay();
    }

    void OnCall()
    {
        if (currentInput == correctCode)
        {
            
            if (objectToActivate != null)
                objectToActivate.SetActive(false);
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