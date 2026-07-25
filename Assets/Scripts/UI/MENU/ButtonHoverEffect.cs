using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Selectable))]
public sealed class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image icon;

    [Header("Style")]
    [SerializeField] private ButtonHoverStyle style;

    [Header("Sound")]
    [SerializeField] private FMODUnity.EventReference hoverEvent;
    [SerializeField] private FMODUnity.EventReference clickEvent;

    private Coroutine _tweenCoroutine;

    // Evita que el mouse "pise" el estado que puso la navegación por teclado
    // (por ejemplo si el mouse está quieto sobre otro botón mientras navegás con W/S).
    private bool _isKeyboardHighlighted;

    public void OnPointerEnter(PointerEventData _)
    {
        if (_isKeyboardHighlighted) return;
        TransitionTo(isHover: true);
        FMODUnity.RuntimeManager.PlayOneShot(hoverEvent);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (_isKeyboardHighlighted) return;
        TransitionTo(isHover: false);
    }

    public void PlayClick()
    {
        if (!clickEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(clickEvent);
    }


    public void SetHighlighted(bool highlighted, bool playSound = false)
    {
        _isKeyboardHighlighted = highlighted;
        TransitionTo(highlighted);

        if (highlighted && playSound && !hoverEvent.IsNull)
            FMODUnity.RuntimeManager.PlayOneShot(hoverEvent);
    }


    public void FlashDenied()
    {
        if (style == null) return;

        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);
        _tweenCoroutine = StartCoroutine(FlashDeniedRoutine());
    }

    private IEnumerator FlashDeniedRoutine()
    {
        Color startBg = backgroundImage != null ? backgroundImage.color : Color.clear;
        Color startText = buttonText != null ? buttonText.color : Color.white;
        Color currentIcon = icon != null ? icon.color : Color.white;

        float inDuration = style.tweenDuration;
        for (float t = 0f; t < inDuration; t += Time.unscaledDeltaTime)
        {
            float progress = t / inDuration;
            ApplyColors(
                Color.Lerp(startBg, style.deniedBackground, progress),
                Color.Lerp(startText, style.deniedText, progress),
                currentIcon
            );
            yield return null;
        }
        ApplyColors(style.deniedBackground, style.deniedText, currentIcon);

        yield return new WaitForSecondsRealtime(style.deniedHoldDuration);

        Color returnBg = _isKeyboardHighlighted ? style.hoverBackground : style.normalBackground;
        Color returnText = _isKeyboardHighlighted ? style.hoverText : style.normalText;
        Color returnIcon = _isKeyboardHighlighted ? style.hoverIconColor : style.normalIconColor;
        for (float t = 0f; t < inDuration; t += Time.unscaledDeltaTime)
        {
            float progress = t / inDuration;
            ApplyColors(
                Color.Lerp(style.deniedBackground, returnBg, progress),
                Color.Lerp(style.deniedText, returnText, progress),
                Color.Lerp(currentIcon, returnIcon, progress)
            );
            yield return null;
        }
        ApplyColors(returnBg, returnText, returnIcon);
        _tweenCoroutine = null;
    }

    private void TransitionTo(bool isHover)
    {
        if (style == null) return;

        Color targetBg = isHover ? style.hoverBackground : style.normalBackground;
        Color targetText = isHover ? style.hoverText : style.normalText;
        Color targetIcon = isHover ? style.hoverIconColor : style.normalIconColor;

        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);


        if (!isActiveAndEnabled)
        {
            ApplyColors(targetBg, targetText, targetIcon);
            _tweenCoroutine = null;
            return;
        }

        _tweenCoroutine = StartCoroutine(TweenColors(targetBg, targetText, targetIcon, style.tweenDuration));
    }

    private IEnumerator TweenColors(Color targetBg, Color targetText, Color targetIcon, float duration)
    {
        Color startBg = backgroundImage != null ? backgroundImage.color : Color.clear;
        Color startText = buttonText != null ? buttonText.color : Color.white;
        Color startIcon = icon != null ? icon.color : Color.white;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float progress = t / duration;
            ApplyColors(
                Color.Lerp(startBg, targetBg, progress),
                Color.Lerp(startText, targetText, progress),
                Color.Lerp(startIcon, targetIcon, progress)
            );
            yield return null;
        }

        ApplyColors(targetBg, targetText, targetIcon);
        _tweenCoroutine = null;
    }

    private void ApplyColors(Color bg, Color text, Color iconColor)
    {
        if (backgroundImage != null) backgroundImage.color = bg;
        if (buttonText != null) buttonText.color = text;
        if (icon != null) icon.color = iconColor;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (style == null)
            Debug.LogWarning($"[{nameof(ButtonHoverEffect)}] No ButtonHoverStyle assigned on '{name}'.", this);
    }
#endif
}