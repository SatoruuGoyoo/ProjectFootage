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

    // Etiqueta limpia del botón. El sufijo de "no disponible" se arma siempre a
    // partir de esta copia y nunca del texto actual, así aplicarlo dos veces da
    // el mismo string y no se puede apilar.
    private string _pristineLabel;
    private TextWrappingModes _wrappingBeforeDenied;
    private bool _deniedLabelApplied;

    private void Awake()
    {
        if (buttonText == null) return;

        _pristineLabel = buttonText.text;

        // Si un reload de dominio cortó un flash a la mitad, el sufijo queda
        // pegado al texto serializado. Acá se lo saca.
        string suffix = BuildDeniedSuffix();
        if (string.IsNullOrEmpty(suffix)) return;

        while (_pristineLabel.EndsWith(suffix))
            _pristineLabel = _pristineLabel.Substring(0, _pristineLabel.Length - suffix.Length);

        if (buttonText.text != _pristineLabel)
            buttonText.text = _pristineLabel;
    }

    private string BuildDeniedSuffix()
    {
        if (style == null || string.IsNullOrEmpty(style.deniedSuffix)) return null;
        return "<color=#" + ColorUtility.ToHtmlStringRGB(style.deniedSuffixColor) + ">"
             + style.deniedSuffix + "</color>";
    }

    private void ApplyDeniedLabel()
    {
        if (buttonText == null || _pristineLabel == null) return;

        string suffix = BuildDeniedSuffix();
        if (string.IsNullOrEmpty(suffix)) return;

        if (!_deniedLabelApplied)
        {
            _wrappingBeforeDenied = buttonText.textWrappingMode;
            _deniedLabelApplied = true;
        }

        buttonText.textWrappingMode = TextWrappingModes.NoWrap;
        buttonText.text = _pristineLabel + suffix;
    }

    private void RestoreLabel()
    {
        if (!_deniedLabelApplied || buttonText == null) return;

        buttonText.text = _pristineLabel;
        buttonText.textWrappingMode = _wrappingBeforeDenied;
        _deniedLabelApplied = false;
    }

    // Si el botón se apaga en medio del flash, la corrutina muere y el sufijo
    // quedaría pegado a la etiqueta.
    private void OnDisable() => RestoreLabel();

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
        RestoreLabel();
        _tweenCoroutine = StartCoroutine(FlashDeniedRoutine());
    }

    private IEnumerator FlashDeniedRoutine()
    {
        ApplyDeniedLabel();

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
        RestoreLabel();
        _tweenCoroutine = null;
    }

    private void TransitionTo(bool isHover)
    {
        if (style == null) return;

        Color targetBg = isHover ? style.hoverBackground : style.normalBackground;
        Color targetText = isHover ? style.hoverText : style.normalText;
        Color targetIcon = isHover ? style.hoverIconColor : style.normalIconColor;

        // Si veníamos de un rechazo interrumpido, el sufijo seguiría pegado.
        if (_tweenCoroutine != null) StopCoroutine(_tweenCoroutine);
        RestoreLabel();

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