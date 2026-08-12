using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Slider))]
public sealed class SliderHoverEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image handleImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Style")]
    [SerializeField] private SliderHoverStyle style;

    [Header("Sound")]
    [SerializeField] private FMODUnity.EventReference hoverEvent;

    private Coroutine _tweenCoroutine;
    private bool _isHighlighted;

    public void SetHighlighted(bool highlighted, bool playSound = false)
    {
        _isHighlighted = highlighted;
        TransitionTo(highlighted);

        if (highlighted && playSound && !hoverEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(hoverEvent);
    }

    private void OnDisable()
    {
        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);
        ApplyState(_isHighlighted);
        _tweenCoroutine = null;
    }

    private void TransitionTo(bool isHover)
    {
        if (style == null) return;

        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);

        if (!isActiveAndEnabled)
        {
            ApplyState(isHover);
            _tweenCoroutine = null;
            return;
        }

        _tweenCoroutine = StartCoroutine(TweenColors(isHover));
    }

    private IEnumerator TweenColors(bool isHover)
    {
        Color targetFill = isHover ? style.hoverFill : style.normalFill;
        Color targetHandle = isHover ? style.hoverHandle : style.normalHandle;
        Color targetBg = isHover ? style.hoverBackground : style.normalBackground;
        Color targetLabel = isHover ? style.hoverLabel : style.normalLabel;

        Color startFill = fillImage != null ? fillImage.color : Color.clear;
        Color startHandle = handleImage != null ? handleImage.color : Color.clear;
        Color startBg = backgroundImage != null ? backgroundImage.color : Color.clear;
        Color startLabel = label != null ? label.color : Color.clear;

        float duration = style.tweenDuration;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float p = t / duration;
            ApplyColors(
                Color.Lerp(startFill, targetFill, p),
                Color.Lerp(startHandle, targetHandle, p),
                Color.Lerp(startBg, targetBg, p),
                Color.Lerp(startLabel, targetLabel, p)
            );
            yield return null;
        }

        ApplyColors(targetFill, targetHandle, targetBg, targetLabel);
        _tweenCoroutine = null;
    }

    private void ApplyState(bool isHover)
    {
        if (style == null) return;
        ApplyColors(
            isHover ? style.hoverFill : style.normalFill,
            isHover ? style.hoverHandle : style.normalHandle,
            isHover ? style.hoverBackground : style.normalBackground,
            isHover ? style.hoverLabel : style.normalLabel
        );
    }

    private void ApplyColors(Color fill, Color handle, Color bg, Color labelColor)
    {
        if (fillImage != null) fillImage.color = fill;
        if (handleImage != null) handleImage.color = handle;
        if (backgroundImage != null) backgroundImage.color = bg;
        if (label != null) label.color = labelColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (style == null)
            Debug.LogWarning($"[{nameof(SliderHoverEffect)}] No SliderHoverStyle assigned on '{name}'.", this);
    }
#endif
}