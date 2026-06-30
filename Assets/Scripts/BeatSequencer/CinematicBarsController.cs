using System.Collections;
using UnityEngine;

public class CinematicBarsController : MonoBehaviour
{
    public static CinematicBarsController Instance { get; private set; }

    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    private const float TopVisible = 0f;
    private const float TopHidden = 200f;
    private const float BottomVisible = 0f;
    private const float BottomHidden = -200f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        topBar.gameObject.SetActive(false);
        bottomBar.gameObject.SetActive(false);
    }

    public IEnumerator Show(float duration, AnimationCurve curve)
    {
        topBar.gameObject.SetActive(true);
        bottomBar.gameObject.SetActive(true);

        topBar.anchoredPosition = new Vector2(0, TopHidden);
        bottomBar.anchoredPosition = new Vector2(0, BottomHidden);

        yield return Animate(TopHidden, TopVisible, BottomHidden, BottomVisible, duration, curve);
    }

    public IEnumerator Hide(float duration, AnimationCurve curve)
    {
        yield return Animate(TopVisible, TopHidden, BottomVisible, BottomHidden, duration, curve);

        topBar.gameObject.SetActive(false);
        bottomBar.gameObject.SetActive(false);
    }

    private IEnumerator Animate(float topFrom, float topTo, float bottomFrom, float bottomTo, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = curve.Evaluate(Mathf.Clamp01(elapsed / duration));

            topBar.anchoredPosition = new Vector2(0, Mathf.Lerp(topFrom, topTo, t));
            bottomBar.anchoredPosition = new Vector2(0, Mathf.Lerp(bottomFrom, bottomTo, t));

            yield return null;
        }

        topBar.anchoredPosition = new Vector2(0, topTo);
        bottomBar.anchoredPosition = new Vector2(0, bottomTo);
    }
}