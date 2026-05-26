using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Slideshow para un quad 3D en URP.
/// Requiere un material con shader URP Unlit o Lit (usa _BaseMap y _BaseColor).
/// Si usás un shader custom, cambiá los nombres en los campos del Inspector.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class SlideShow : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // Inspector
    // ──────────────────────────────────────────────

    [Header("Slides")]
    [Tooltip("Texturas en orden. Asignalas directo desde el Inspector.")]
    [SerializeField] private Texture2D[] slides;

    [Header("Timing")]
    [Tooltip("Cuántos segundos se muestra cada slide (solo en autoPlay).")]
    [SerializeField, Min(0.05f)] private float slideDuration = 3f;

    [Tooltip("Duración total del fade-to-black entre slides (0 = corte directo).")]
    [SerializeField, Min(0f)] private float transitionDuration = 0.5f;

    [Header("Comportamiento")]
    [Tooltip("Arranca solo al Start.")]
    [SerializeField] private bool autoPlay = true;

    [Tooltip("Vuelve al principio al terminar.")]
    [SerializeField] private bool loop = true;

    [Header("Shader (URP Unlit/Lit por defecto)")]
    [Tooltip("Nombre de la propiedad de textura en el shader.")]
    [SerializeField] private string texturePropertyName = "_BaseMap";

    [Tooltip("Nombre de la propiedad de color/tint en el shader.")]
    [SerializeField] private string colorPropertyName = "_BaseColor";

    [Header("Eventos")]
    public UnityEvent<int> OnSlideChanged;   // dispara con el índice del slide nuevo

    // ──────────────────────────────────────────────
    // Estado interno
    // ──────────────────────────────────────────────

    private Renderer _renderer;
    private MaterialPropertyBlock _mpb;
    private int _currentIndex;
    private bool _isTransitioning;
    private bool _autoPlaying;
    private Coroutine _autoPlayCoroutine;
    private int _texPropId;
    private int _colPropId;

    
    public int CurrentIndex => _currentIndex;
    public int SlideCount => slides != null ? slides.Length : 0;
    public bool IsTransitioning => _isTransitioning;
    public bool IsAutoPlaying => _autoPlaying;

   
    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _mpb = new MaterialPropertyBlock();
        _texPropId = Shader.PropertyToID(texturePropertyName);
        _colPropId = Shader.PropertyToID(colorPropertyName);
    }

    private void Start()
    {
        if (!ValidateSlides()) return;

        ShowSlideImmediate(0);

        if (autoPlay) StartAutoPlay();
    }

    public void Next()
    {
        int next = _currentIndex + 1;
        if (next >= slides.Length)
        {
            if (!loop) return;
            next = 0;
        }
        RequestTransition(next);
    }


    public void Previous()
    {
        int prev = _currentIndex - 1;
        if (prev < 0)
        {
            if (!loop) return;
            prev = slides.Length - 1;
        }
        RequestTransition(prev);
    }


    public void GoTo(int index)
    {
        if (index < 0 || index >= slides.Length)
        {
            Debug.LogWarning($"[SlideShow] Índice {index} fuera de rango (0–{slides.Length - 1}).");
            return;
        }
        RequestTransition(index);
    }

    public void StartAutoPlay()
    {
        if (_autoPlaying) return;
        _autoPlaying = true;
        _autoPlayCoroutine = StartCoroutine(AutoPlayLoop());
    }


    public void StopAutoPlay()
    {
        _autoPlaying = false;
        if (_autoPlayCoroutine != null)
        {
            StopCoroutine(_autoPlayCoroutine);
            _autoPlayCoroutine = null;
        }
    }

   
    private void RequestTransition(int targetIndex)
    {
        if (_isTransitioning) return;
        if (targetIndex == _currentIndex) return;
        StartCoroutine(TransitionTo(targetIndex));
    }

    private IEnumerator AutoPlayLoop()
    {
        while (_autoPlaying)
        {
            yield return new WaitForSeconds(slideDuration);

            if (!_autoPlaying) yield break;

            int next = _currentIndex + 1;
            if (next >= slides.Length)
            {
                if (!loop) { _autoPlaying = false; yield break; }
                next = 0;
            }

          
            yield return new WaitUntil(() => !_isTransitioning);

            yield return TransitionTo(next);
        }
    }

    private IEnumerator TransitionTo(int targetIndex)
    {
        _isTransitioning = true;

        if (transitionDuration > 0f)
        {
            float half = transitionDuration * 0.5f;

            // Fade OUT — blanco a negro
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                SetTint(Color.Lerp(Color.white, Color.black, t / half));
                yield return null;
            }
            SetTint(Color.black);
        }

        ShowSlideImmediate(targetIndex);

        if (transitionDuration > 0f)
        {
            float half = transitionDuration * 0.5f;

         
            for (float t = 0f; t < half; t += Time.deltaTime)
            {
                SetTint(Color.Lerp(Color.black, Color.white, t / half));
                yield return null;
            }
            SetTint(Color.white);
        }

        _isTransitioning = false;
    }

    private void ShowSlideImmediate(int index)
    {
        _currentIndex = index;
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetTexture(_texPropId, slides[index]);
        _renderer.SetPropertyBlock(_mpb);
        OnSlideChanged?.Invoke(index);
    }

    private void SetTint(Color color)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(_colPropId, color);
        _renderer.SetPropertyBlock(_mpb);
    }

    private bool ValidateSlides()
    {
        if (slides == null || slides.Length == 0)
        {
            Debug.LogError("[SlideShow] No hay slides asignados.", this);
            return false;
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _texPropId = Shader.PropertyToID(texturePropertyName);
        _colPropId = Shader.PropertyToID(colorPropertyName);
    }
#endif
}