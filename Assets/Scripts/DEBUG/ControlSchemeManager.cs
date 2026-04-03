using UnityEngine;

public class ControlSchemeManager : MonoBehaviour
{
    public static ControlSchemeManager Instance { get; private set; }

    public enum Scheme { Tank, Modern }

    [SerializeField] private Scheme startingScheme = Scheme.Tank;
    public Scheme CurrentScheme { get; private set; }

    public static event System.Action<Scheme> OnSchemeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        CurrentScheme = startingScheme;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentScheme = CurrentScheme == Scheme.Tank ? Scheme.Modern : Scheme.Tank;
            OnSchemeChanged?.Invoke(CurrentScheme);
            Debug.Log($"[ControlScheme] Switched to: {CurrentScheme}");
        }
    }

    private void OnGUI()
    {
        GUIStyle style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        style.normal.textColor = Color.yellow;

        GUI.Label(new Rect(10, 10, 400, 30),
            $"Controls: {CurrentScheme}  (M to toggle)", style);
    }
}