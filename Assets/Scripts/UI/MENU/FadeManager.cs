using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public event Action OnFadeOutComplete;
    public event Action OnFadeInComplete;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float defaultFadeDuration = 0.8f;

    public bool IsBusy { get; private set; }
    public bool IsFadedOut { get; private set; }

    private Image _overlay;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildOverlayUI();
        SetOverlayAlpha(0f);
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        IsBusy = true;
        float d = duration < 0f ? defaultFadeDuration : duration;
        yield return TweenOverlay(0f, 1f, d);
        IsFadedOut = true;
        IsBusy = false;
        OnFadeOutComplete?.Invoke();
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        IsBusy = true;
        float d = duration < 0f ? defaultFadeDuration : duration;
        yield return TweenOverlay(1f, 0f, d);
        IsFadedOut = false;
        IsBusy = false;
        OnFadeInComplete?.Invoke();
    }

    public void SetBlackInstant()
    {
        SetOverlayAlpha(1f);
        IsFadedOut = true;
    }

    private IEnumerator TweenOverlay(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            SetOverlayAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetOverlayAlpha(to);
    }

    private void SetOverlayAlpha(float alpha)
    {
        Color c = _overlay.color;
        c.a = alpha;
        _overlay.color = c;
    }

    private void BuildOverlayUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        gameObject.AddComponent<GraphicRaycaster>();

        _overlay = CreateFullscreenImage("FadeOverlay", Color.black);
    }

    private Image CreateFullscreenImage(string goName, Color color)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);

        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        return img;
    }
}