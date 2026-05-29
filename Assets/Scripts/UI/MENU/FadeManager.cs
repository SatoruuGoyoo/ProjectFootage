using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public sealed class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }

    public event Action OnFadeOutComplete;
    public event Action OnFadeInComplete;
    public event Action<string> OnSceneTransitionStart;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float defaultFadeDuration = 0.8f;
    [SerializeField, Min(0f)] private float defaultHoldDuration = 2.0f;
    [SerializeField, Min(0f)] private float textFadeDuration = 0.4f;

    [Header("Intro Text Style")]
    [SerializeField, Min(1f)] private float fontSize = 28f;
    [SerializeField] private Color textColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    public bool IsBusy { get; private set; }

    private Image _overlay;
    private TextMeshProUGUI _label;

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
        _label.alpha = 0f;
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        float d = duration < 0f ? defaultFadeDuration : duration;
        yield return TweenOverlay(0f, 1f, d);
        OnFadeOutComplete?.Invoke();
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        float d = duration < 0f ? defaultFadeDuration : duration;
        yield return TweenOverlay(1f, 0f, d);
        OnFadeInComplete?.Invoke();
    }

    public void FadeToScene(
        string sceneName,
        string introText = "",
        float fadeOutDuration = -1f,
        float holdDuration = -1f,
        float fadeInDuration = -1f)
    {
        if (IsBusy) return;

        var config = new TransitionConfig(
            sceneName,
            introText,
            fadeOutDuration < 0f ? defaultFadeDuration : fadeOutDuration,
            holdDuration < 0f ? defaultHoldDuration : holdDuration,
            fadeInDuration < 0f ? defaultFadeDuration : fadeInDuration
        );

        StartCoroutine(RunTransition(config));
    }

    private IEnumerator RunTransition(TransitionConfig config)
    {
        IsBusy = true;
        OnSceneTransitionStart?.Invoke(config.SceneName);

        yield return TweenOverlay(0f, 1f, config.FadeOutDuration);
        OnFadeOutComplete?.Invoke();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(config.SceneName);
        loadOp.allowSceneActivation = false;

        yield return RunIntroText(config.IntroText, config.HoldDuration);

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();

        if (GameWarmup.Instance != null)
        {
            while (!GameWarmup.Instance.IsFinished)
                yield return null;
        }

        yield return TweenOverlay(1f, 0f, config.FadeInDuration);
        OnFadeInComplete?.Invoke();
        IsBusy = false;
    }

    private IEnumerator RunIntroText(string text, float hold)
    {
        if (string.IsNullOrEmpty(text))
        {
            yield return new WaitForSeconds(hold * 0.3f);
            yield break;
        }

        _label.text = text;
        yield return TweenText(0f, 1f, textFadeDuration);
        yield return new WaitForSeconds(hold);
        yield return TweenText(1f, 0f, textFadeDuration);
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

    private IEnumerator TweenText(float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _label.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _label.alpha = to;
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
        _label = CreateCenteredLabel("FadeLabel");
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

    private TextMeshProUGUI CreateCenteredLabel(string goName)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = fontSize;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.1f, 0.35f);
        rt.anchorMax = new Vector2(0.9f, 0.65f);
        rt.sizeDelta = Vector2.zero;

        return tmp;
    }

    private readonly struct TransitionConfig
    {
        public readonly string SceneName;
        public readonly string IntroText;
        public readonly float FadeOutDuration;
        public readonly float HoldDuration;
        public readonly float FadeInDuration;

        public TransitionConfig(
            string sceneName,
            string introText,
            float fadeOutDuration,
            float holdDuration,
            float fadeInDuration)
        {
            SceneName = sceneName;
            IntroText = introText;
            FadeOutDuration = fadeOutDuration;
            HoldDuration = holdDuration;
            FadeInDuration = fadeInDuration;
        }
    }
}