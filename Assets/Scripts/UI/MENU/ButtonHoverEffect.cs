using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Animates a button's background and text colors on pointer enter/exit.
/// Assign a <see cref="ButtonHoverStyle"/> asset for shared, reusable styling.
/// </summary>
[RequireComponent(typeof(Selectable))]
public sealed class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Style")]
    [SerializeField] private ButtonHoverStyle style;

    private Coroutine _tweenCoroutine;

    // ── Pointer Events ────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _) => TransitionTo(isHover: true);
    public void OnPointerExit(PointerEventData _) => TransitionTo(isHover: false);

    // ── Tween ─────────────────────────────────────────────────────────────

    private void TransitionTo(bool isHover)
    {
        if (style == null) return;

        Color targetBg = isHover ? style.hoverBackground : style.normalBackground;
        Color targetText = isHover ? style.hoverText : style.normalText;

        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);
        _tweenCoroutine = StartCoroutine(TweenColors(targetBg, targetText, style.tweenDuration));
    }

    private IEnumerator TweenColors(Color targetBg, Color targetText, float duration)
    {
        Color startBg = backgroundImage != null ? backgroundImage.color : Color.clear;
        Color startText = buttonText != null ? buttonText.color : Color.white;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float progress = t / duration;
            ApplyColors(Color.Lerp(startBg, targetBg, progress),
                        Color.Lerp(startText, targetText, progress));
            yield return null;
        }

        ApplyColors(targetBg, targetText);
        _tweenCoroutine = null;
    }

    private void ApplyColors(Color bg, Color text)
    {
        if (backgroundImage != null) backgroundImage.color = bg;
        if (buttonText != null) buttonText.color = text;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (style == null)
            Debug.LogWarning($"[{nameof(ButtonHoverEffect)}] No ButtonHoverStyle assigned on '{name}'.", this);
    }
#endif
}