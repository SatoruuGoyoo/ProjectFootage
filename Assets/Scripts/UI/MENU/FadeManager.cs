using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
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
    [SerializeField, Min(0f)] private float videoSkipHoldDuration = 0.5f;

    [Header("Intro Text Style")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField, Min(1f)] private float fontSize = 28f;
    [SerializeField] private Color textColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    [Header("Intro Video")]
    [SerializeField] private VideoClip defaultIntroVideoClip;
    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private VideoClip menuIntroVideoClip;
    [SerializeField] private string gameSceneName = "Juego";
    [SerializeField] private VideoClip gameIntroVideoClip;

    public bool IsBusy { get; private set; }

    private Image _overlay;
    private TextMeshProUGUI _label;
    private RawImage _videoDisplay;
    private VideoPlayer _videoPlayer;

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
        _videoDisplay.gameObject.SetActive(false);
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
        float fadeInDuration = -1f,
        VideoClip introVideo = null)
    {
        if (IsBusy) return;

        var config = new TransitionConfig(
            sceneName,
            introText,
            fadeOutDuration < 0f ? defaultFadeDuration : fadeOutDuration,
            holdDuration < 0f ? defaultHoldDuration : holdDuration,
            fadeInDuration < 0f ? defaultFadeDuration : fadeInDuration,
            introVideo != null ? introVideo : ResolveIntroVideo()
        );

        StartCoroutine(RunTransition(config));
    }

    private VideoClip ResolveIntroVideo()
    {
        string activeScene = SceneManager.GetActiveScene().name;

        if (activeScene == menuSceneName)
            return menuIntroVideoClip;

        if (activeScene == gameSceneName)
            return gameIntroVideoClip;

        return defaultIntroVideoClip;
    }

    private IEnumerator RunTransition(TransitionConfig config)
    {
        IsBusy = true;
        OnSceneTransitionStart?.Invoke(config.SceneName);

        // 1. Fade Out a negro
        yield return TweenOverlay(0f, 1f, config.FadeOutDuration);
        OnFadeOutComplete?.Invoke();

        // 2. Video si hay uno asignado
        if (config.IntroVideo != null)
            yield return RunIntroVideo(config.IntroVideo);

        // 3. Texto
        yield return RunIntroText(config.IntroText, config.HoldDuration);

        // 4. Cargar escena
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(config.SceneName);
        loadOp.allowSceneActivation = false;

        while (loadOp.progress < 0.9f)
            yield return null;

        loadOp.allowSceneActivation = true;

        while (!loadOp.isDone)
            yield return null;

        yield return new WaitForEndOfFrame();

        // 5. Fade In
        yield return TweenOverlay(1f, 0f, config.FadeInDuration);
        OnFadeInComplete?.Invoke();
        IsBusy = false;
    }

    private IEnumerator RunIntroVideo(VideoClip clip)
    {
        _videoDisplay.gameObject.SetActive(true);
        _videoPlayer.clip = clip;
        _videoPlayer.EnableAudioTrack(0, false);
        _videoPlayer.Prepare();

        while (!_videoPlayer.isPrepared)
            yield return null;

        var audioInstance = FMODUnity.RuntimeManager.CreateInstance("event:/MainMenu/Ambient/IntroVideo");
        audioInstance.start();
        // ← ya no se hace release() aquí

        _videoPlayer.Play();

        yield return TweenOverlay(1f, 0f, 0.5f);

        float skipTimer = 0f;
        while (_videoPlayer.isPlaying)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                skipTimer += Time.unscaledDeltaTime;
                if (skipTimer >= videoSkipHoldDuration)
                    break;
            }
            else
            {
                skipTimer = 0f;
            }
            yield return null;
        }

        // ← se detiene aquí, tanto si se skipea como si termina solo
        audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        audioInstance.release();

        yield return TweenOverlay(0f, 1f, 0.5f);

        _videoDisplay.gameObject.SetActive(false);
        _videoPlayer.Stop();
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

        var videoGO = new GameObject("VideoDisplay");
        videoGO.transform.SetParent(transform, false);

        _videoDisplay = videoGO.AddComponent<RawImage>();
        _videoDisplay.raycastTarget = false;

        var rt = videoGO.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        _videoPlayer = videoGO.AddComponent<VideoPlayer>();
        _videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        _videoPlayer.isLooping = false;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        var renderTexture = new RenderTexture(1920, 1080, 0);
        _videoPlayer.targetTexture = renderTexture;
        _videoDisplay.texture = renderTexture;
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
        tmp.font = font;
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
        public readonly VideoClip IntroVideo;

        public TransitionConfig(string sceneName, string introText, float fadeOutDuration, float holdDuration, float fadeInDuration, VideoClip introVideo)
        {
            SceneName = sceneName;
            IntroText = introText;
            FadeOutDuration = fadeOutDuration;
            HoldDuration = holdDuration;
            FadeInDuration = fadeInDuration;
            IntroVideo = introVideo;
        }
    }
}