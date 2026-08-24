using UnityEngine;
using UnityEngine.InputSystem;

public class UINavigationInput
{
    private const float PressThreshold = 0.5f;
    private const float ReleaseThreshold = 0.1f;

    private readonly InputAction _action;
    private bool _xNeutral = true;
    private bool _yNeutral = true;

    public UINavigationInput(InputAction action)
    {
        _action = action;
    }

    public void Reset()
    {
        Vector2 value = Current;
        _xNeutral = Mathf.Abs(value.x) < PressThreshold;
        _yNeutral = Mathf.Abs(value.y) < PressThreshold;
    }

    public Vector2Int Read()
    {
        Vector2 value = Current;
        return new Vector2Int(
            Step(value.x, ref _xNeutral),
            Step(value.y, ref _yNeutral));
    }

    private Vector2 Current => _action != null ? _action.ReadValue<Vector2>() : Vector2.zero;

    private static int Step(float axis, ref bool neutral)
    {
        if (!neutral)
        {
            if (Mathf.Abs(axis) < ReleaseThreshold) neutral = true;
            return 0;
        }

        if (Mathf.Abs(axis) < PressThreshold) return 0;

        neutral = false;
        return axis > 0f ? 1 : -1;
    }
}