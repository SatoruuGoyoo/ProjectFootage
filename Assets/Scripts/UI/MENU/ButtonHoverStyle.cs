using UnityEngine;

/// <summary>
/// Reusable hover style config. Create one asset per button theme via
/// Assets > Create > SM/UI > Button Hover Style.
/// </summary>
[CreateAssetMenu(menuName = "SM/UI/Button Hover Style", fileName = "ButtonHoverStyle")]
public sealed class ButtonHoverStyle : ScriptableObject
{
    [Header("Background")]
    public Color normalBackground = new Color(0f, 0f, 0f, 0f);
    public Color hoverBackground = Color.white;

    [Header("Text")]
    public Color normalText = Color.white;
    public Color hoverText = Color.black;

    [Header("Icon")]
    public Color normalIconColor = Color.white;
    public Color hoverIconColor = Color.black;

    [Header("Denied")]
    public Color deniedBackground = new Color(0.35f, 0.08f, 0.08f, 1f);
    public Color deniedText = Color.white;
    [Min(0f)] public float deniedHoldDuration = 0.25f;

    [Header("Transition")]
    [Min(0f)] public float tweenDuration = 0.12f;
}