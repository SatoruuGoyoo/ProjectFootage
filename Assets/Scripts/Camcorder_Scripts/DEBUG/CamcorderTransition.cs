using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderTransition : MonoBehaviour
{
    [Header("Setup")]
    public Image transitionImage;       // Image fullscreen con material UI/VHSStatic

    [Header("Config")]
    [SerializeField] private float rampUpDuration = 0.25f;    
    [SerializeField] private float holdDuration = 0.05f;      
    [SerializeField] private float rampDownDuration = 0.2f;   

    public bool IsTransitioning { get; private set; } = false;

    private Material staticMaterial;

    private void Awake()
    {
        staticMaterial = transitionImage.material;
    }

    private void Start()
    {
        SetIntensity(0f);
        transitionImage.raycastTarget = false;
    }

    public void Play(Action onSwitch, Action onComplete = null)
    {
        if (IsTransitioning) return;
        StartCoroutine(TransitionRoutine(onSwitch, onComplete));
    }

    private IEnumerator TransitionRoutine(Action onSwitch, Action onComplete)
    {
        IsTransitioning = true;
        transitionImage.raycastTarget = true;

        float elapsed = 0f;
        while (elapsed < rampUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / rampUpDuration;
            SetIntensity(t * t);
            yield return null;
        }
        SetIntensity(1f);

        onSwitch?.Invoke();

        yield return new WaitForSecondsRealtime(holdDuration);
        elapsed = 0f;
        while (elapsed < rampDownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - (elapsed / rampDownDuration);
            SetIntensity(t * t);
            yield return null;
        }
        SetIntensity(0f);

        transitionImage.raycastTarget = false;
        IsTransitioning = false;

        onComplete?.Invoke();
    }

    private void SetIntensity(float intensity)
    {
        staticMaterial.SetFloat("_Intensity", intensity);
    }
}