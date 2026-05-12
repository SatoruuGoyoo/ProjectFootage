using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Singleton persistente de fade. Llamable desde cualquier script con:
///   FadeManager.Instance.FadeToScene("SCN_Integration", "Texto de intro");
///   yield return StartCoroutine(FadeManager.Instance.FadeOut());
///   yield return StartCoroutine(FadeManager.Instance.FadeIn());
/// </summary>
public class FadeManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static FadeManager Instance { get; private set; }

    // ── Config ─────────────────────────────────────────────────────────────
    [Header("Duraciones por defecto (segundos)")]
    [SerializeField] private float defaultFadeDuration = 0.8f;
    [SerializeField] private float defaultHoldDuration = 2.0f;

    [Header("Tipografía del texto de transición")]
    [SerializeField] private float fontSize = 28f;
    [SerializeField] private Color textColor = new Color(0.85f, 0.85f, 0.85f, 1f);

    // ── Referencias internas ───────────────────────────────────────────────
    private Canvas _canvas;
    private Image _overlay;   // panel negro
    private TextMeshProUGUI _label;   // texto de intro
    private float _pendingFadeIn;
    private string _pendingFadeInScene;

    private bool _isBusy = false;     // evita llamadas superpuestas


    private void Awake()
    {
        // Singleton clásico con DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildUI();

        // Arranca transparente
        SetAlpha(0f);
        _label.alpha = 0f;
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        duration = duration < 0 ? defaultFadeDuration : duration;
        yield return StartCoroutine(TweenOverlay(0f, 1f, duration));
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        duration = duration < 0 ? defaultFadeDuration : duration;
        yield return StartCoroutine(TweenOverlay(1f, 0f, duration));
    }

    /// <summary>
    /// Flow completo: FadeOut → [texto opcional] → carga escena → FadeIn.
    /// Llamá esto desde el botón de Play del menú u otro trigger.
    /// </summary>
    /// <param name="sceneName">Nombre exacto de la escena destino</param>
    /// <param name="introText">Texto que aparece en pantalla negra. Vacío = sin texto</param>
    /// <param name="fadeOutDuration">Duración del fade a negro</param>
    /// <param name="holdDuration">Cuánto tiempo se mantiene el texto en pantalla</param>
    /// <param name="fadeInDuration">
    public void FadeToScene(
        string sceneName,
        string introText = "",
        float fadeOutDuration = -1f,
        float holdDuration = -1f,
        float fadeInDuration = -1f)
    {
        if (_isBusy) return;
        StartCoroutine(FadeToSceneRoutine(
            sceneName, introText,
            fadeOutDuration < 0 ? defaultFadeDuration : fadeOutDuration,
            holdDuration < 0 ? defaultHoldDuration : holdDuration,
            fadeInDuration < 0 ? defaultFadeDuration : fadeInDuration
        ));
    }


    private IEnumerator FadeToSceneRoutine(
    string sceneName, string introText,
    float fadeOut, float hold, float fadeIn)
    {
        _isBusy = true;

        // 1. Pantalla → negro
        yield return StartCoroutine(TweenOverlay(0f, 1f, fadeOut));

        // 2. Texto opcional
        if (!string.IsNullOrEmpty(introText))
        {
            _label.text = introText;
            yield return StartCoroutine(TweenText(0f, 1f, 0.4f));
            yield return new WaitForSeconds(hold);
            yield return StartCoroutine(TweenText(1f, 0f, 0.4f));
        }
        else
        {
            yield return new WaitForSeconds(hold * 0.3f);
        }


        _pendingFadeIn = fadeIn;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(sceneName);

  
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; 
        StartCoroutine(FadeInAfterLoad(_pendingFadeIn));
    }

    private IEnumerator FadeInAfterLoad(float duration)
    {
       
        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(TweenOverlay(1f, 0f, duration));

        _isBusy = false;
    }

    private IEnumerator TweenOverlay(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // unscaled por si el juego está pausadoz
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }

    private IEnumerator TweenText(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            _label.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        _label.alpha = to;
    }

    private void SetAlpha(float a)
    {
        Color c = _overlay.color;
        c.a = a;
        _overlay.color = c;
    }

   

    private void BuildUI()
    {
        // Canvas
        _canvas = gameObject.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 999; // siempre encima de todo

        gameObject.AddComponent<CanvasScaler>().uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

      
        GameObject overlayGO = new GameObject("FadeOverlay");
        overlayGO.transform.SetParent(_canvas.transform, false);

        _overlay = overlayGO.AddComponent<Image>();
        _overlay.color = Color.black;
        _overlay.raycastTarget = false;

        RectTransform ort = overlayGO.GetComponent<RectTransform>();
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.sizeDelta = Vector2.zero;

      
        GameObject textGO = new GameObject("FadeLabel");
        textGO.transform.SetParent(_canvas.transform, false);

        _label = textGO.AddComponent<TextMeshProUGUI>();
        _label.fontSize = fontSize;
        _label.color = textColor;
        _label.alignment = TextAlignmentOptions.Center;
        _label.raycastTarget = false;
        

        RectTransform trt = textGO.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0.1f, 0.35f);
        trt.anchorMax = new Vector2(0.9f, 0.65f);
        trt.sizeDelta = Vector2.zero;
    }
}