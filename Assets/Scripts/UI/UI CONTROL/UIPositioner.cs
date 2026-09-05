using UnityEngine;

public class UIPositioner : MonoBehaviour
{
    [SerializeField] private RectTransform upperLeft;
    [SerializeField] private RectTransform upperCenter;
    [SerializeField] private RectTransform upperRight;
    [SerializeField] private RectTransform middleLeft;
    [SerializeField] private RectTransform middleCenter;
    [SerializeField] private RectTransform middleRight;
    [SerializeField] private RectTransform lowerLeft;
    [SerializeField] private RectTransform lowerCenter;
    [SerializeField] private RectTransform lowerRight;

    public enum ScreenPosition
    {
        UpperLeft, UpperCenter, UpperRight,
        MiddleLeft, MiddleCenter, MiddleRight,
        LowerLeft, LowerCenter, LowerRight
    }

    private RectTransform _rect;

    private void Awake() => _rect = GetComponent<RectTransform>();

    public void SetPosition(ScreenPosition position)
    {
        if (_rect == null) _rect = GetComponent<RectTransform>();

        RectTransform anchor = position switch
        {
            ScreenPosition.UpperLeft => upperLeft,
            ScreenPosition.UpperCenter => upperCenter,
            ScreenPosition.UpperRight => upperRight,
            ScreenPosition.MiddleLeft => middleLeft,
            ScreenPosition.MiddleCenter => middleCenter,
            ScreenPosition.MiddleRight => middleRight,
            ScreenPosition.LowerLeft => lowerLeft,
            ScreenPosition.LowerCenter => lowerCenter,
            ScreenPosition.LowerRight => lowerRight,
            _ => lowerCenter
        };

        if (anchor == null) return;

        _rect.position = anchor.position;
    }
}