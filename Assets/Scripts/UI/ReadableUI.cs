using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ReadableUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup container;
    [SerializeField] private CanvasGroup bgGroup;
    [SerializeField] private Image itemSprite;
    [SerializeField] private TMP_Text textField;

    [Header("Animation")]
    [SerializeField] private float bgFadeInDuration = 0.4f;
    [SerializeField] private float bgTargetAlpha = 0.7f;

    private Coroutine _animCoroutine;

    private void Awake() => ForceHide();

    private void OnEnable()
    {
        GameEvents.OnReadableOpened += OnOpened;
        GameEvents.OnReadableClosed += OnClosed;
    }

    private void OnDisable()
    {
        GameEvents.OnReadableOpened -= OnOpened;
        GameEvents.OnReadableClosed -= OnClosed;
    }

    private void OnOpened(Sprite sprite, string text)
    {
        if (!UILayerManager.TryShow(UILayerManager.Layer.Readable, ForceHide)) return;

        if (itemSprite != null) itemSprite.sprite = sprite;
        if (textField != null) textField.SetText(text);

        if (_animCoroutine != null) StopCoroutine(_animCoroutine);
        _animCoroutine = StartCoroutine(AnimateIn());
    }

    private void OnClosed()
    {
        UILayerManager.Release(UILayerManager.Layer.Readable);
        if (_animCoroutine != null) { StopCoroutine(_animCoroutine); _animCoroutine = null; }
        ForceHide();
    }

    private void ForceHide()
    {
        UILayerManager.Release(UILayerManager.Layer.Readable);
        if (_animCoroutine != null) { StopCoroutine(_animCoroutine); _animCoroutine = null; }
        SetVisible(false);
    }

    private IEnumerator AnimateIn()
    {
        if (bgGroup != null) bgGroup.alpha = 0f;
        if (itemSprite != null) itemSprite.gameObject.SetActive(false);
        if (textField != null) textField.gameObject.SetActive(false);

        SetVisible(true);

        float t = 0f;
        while (t < bgFadeInDuration)
        {
            t += Time.deltaTime;
            if (bgGroup != null) bgGroup.alpha = Mathf.Lerp(0f, bgTargetAlpha, t / bgFadeInDuration);
            yield return null;
        }
        if (bgGroup != null) bgGroup.alpha = bgTargetAlpha;

        if (itemSprite != null) itemSprite.gameObject.SetActive(true);
        if (textField != null) textField.gameObject.SetActive(true);

        _animCoroutine = null;
    }

    private void SetVisible(bool visible)
    {
        if (container == null) return;
        container.alpha = visible ? 1f : 0f;
        container.interactable = visible;
        container.blocksRaycasts = visible;
    }
}