using UnityEngine;

public class ControlSchemeManager : MonoBehaviour
{
    [SerializeField] private ControlScheme startingScheme = ControlScheme.Tank;
    public ControlScheme CurrentScheme { get; private set; }

    private void Start()
    {
        CurrentScheme = startingScheme;
        GameEvents.ControllerSchemeChanged(CurrentScheme);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentScheme = CurrentScheme == ControlScheme.Tank ? ControlScheme.Modern : ControlScheme.Tank;
            GameEvents.ControllerSchemeChanged(CurrentScheme);
            Debug.Log($"[ControlScheme] Switched to: {CurrentScheme}");
        }
    }
}