using UnityEngine;

[CreateAssetMenu(fileName = "SliderHoverStyle", menuName = "UI/Slider Hover Style")]
public sealed class SliderHoverStyle : ScriptableObject
{
    [Header("Fill")]
    public Color normalFill = new Color(0.5f, 0.5f, 0.5f, 1f);
    public Color hoverFill = new Color(1f, 0.3f, 0.3f, 1f);

    [Header("Handle")]
    public Color normalHandle = Color.white;
    public Color hoverHandle = Color.white;

    [Header("Background")]
    public Color normalBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
    public Color hoverBackground = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Label")]
    public Color normalLabel = new Color(0.6f, 0.6f, 0.6f, 1f);
    public Color hoverLabel = Color.white;

    [Header("Tween")]
    public float tweenDuration = 0.15f;
}