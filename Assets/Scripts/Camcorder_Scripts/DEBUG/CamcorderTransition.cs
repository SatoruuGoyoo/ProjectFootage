using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CamcorderTransition : MonoBehaviour
{
    [Header("Setup")]
    public Image transitionImage;       // Image fullscreen con material UI/VHSStatic

    [Header("Config")]
    [SerializeField] private float rampUpDuration = 0.25f;    // Estática subiendo
    [SerializeField] private float holdDuration = 0.05f;      // Mantener cubierto
    [SerializeField] private float rampDownDuration = 0.2f;   // Estática bajando

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

        // Fase 1: Estática se intensifica de 0 a 1
        float elapsed = 0f;
        while (elapsed < rampUpDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / rampUpDuration;
            // Curva acelerada para que el final sea más agresivo
            SetIntensity(t * t);
            yield return null;
        }
        SetIntensity(1f);

        // Pantalla cubierta de estática — hacer el switch
        onSwitch?.Invoke();

        yield return new WaitForSecondsRealtime(holdDuration);

        // Fase 2: Estática se disipa de 1 a 0
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