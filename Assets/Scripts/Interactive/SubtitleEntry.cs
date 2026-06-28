using System;

[Serializable]
public class SubtitleEntry
{
    public string text;
    public float duration = 3f;
    public UIPositioner.ScreenPosition position = UIPositioner.ScreenPosition.LowerCenter;
}