using UnityEngine;
using System.Collections;

public class HorrorFlickerLight : MonoBehaviour
{
    // ─────────────────────────────────────────
    // REFERENCIAS
    // ─────────────────────────────────────────
    [Header("Referencia")]
    [Tooltip("Si está vacío, busca el Light en este mismo GameObject.")]
    public Light targetLight;

    // ─────────────────────────────────────────
    // MODO
    // ─────────────────────────────────────────
    [Header("Modo de Flicker")]
    public FlickerMode mode = FlickerMode.Random;

    public enum FlickerMode
    {
        Random,       // cortes irregulares, muy orgánico
        Rhythmic,     // pulso constante tipo estroboscópico
        Malfunction,  // largos períodos estable + ráfagas de flicker
        Noise,        // Perlin noise en intensidad, muy suave
    }

    // ─────────────────────────────────────────
    // SINCRONIZACIÓN
    // ─────────────────────────────────────────
    [Header("Sincronización")]
    [Tooltip("Todas las luces con el mismo SyncGroup flickean juntas.")]
    public bool useSyncGroup = false;
    public int syncGroup = 0;
    [Tooltip("Offset de fase en segundos para sincronía con desplazamiento.")]
    public float syncPhaseOffset = 0f;

    // ─────────────────────────────────────────
    // INTENSIDAD
    // ─────────────────────────────────────────
    [Header("Intensidad")]
    public float baseIntensity = 1f;
    [Range(0f, 1f)] public float minIntensityRatio = 0f;   // 0 = apagado total
    [Range(0f, 1f)] public float maxIntensityRatio = 1f;

    // ─────────────────────────────────────────
    // RANDOM
    // ─────────────────────────────────────────
    [Header("Modo Random")]
    public float randomOnTimeMin = 0.05f;
    public float randomOnTimeMax = 0.3f;
    public float randomOffTimeMin = 0.02f;
    public float randomOffTimeMax = 0.15f;
    [Tooltip("Cada cuántos segundos ocurre un evento de flicker.")]
    public float randomEventIntervalMin = 1f;
    public float randomEventIntervalMax = 4f;
    [Tooltip("Cuántos cortes rápidos por evento.")]
    public int flickersPerEventMin = 2;
    public int flickersPerEventMax = 7;

    // ─────────────────────────────────────────
    // RHYTHMIC
    // ─────────────────────────────────────────
    [Header("Modo Rhythmic")]
    public float rhythmOnDuration = 0.1f;
    public float rhythmOffDuration = 0.1f;
    [Tooltip("Número de ciclos antes de una pausa. 0 = infinito.")]
    public int rhythmBurstCount = 0;
    public float rhythmPauseMin = 1f;
    public float rhythmPauseMax = 3f;

    // ─────────────────────────────────────────
    // MALFUNCTION
    // ─────────────────────────────────────────
    [Header("Modo Malfunction")]
    public float stableTimeMin = 2f;
    public float stableTimeMax = 6f;
    public float malfunctionTimeMin = 0.5f;
    public float malfunctionTimeMax = 2f;
    [Tooltip("Intensidad reducida durante malfunción (ratio).")]
    [Range(0f, 1f)] public float malfunctionBaseRatio = 0.4f;

    // ─────────────────────────────────────────
    // NOISE
    // ─────────────────────────────────────────
    [Header("Modo Noise")]
    public float noiseSpeed = 1.5f;
    public float noiseIntensity = 0.4f;   // cuánto varía respecto a baseIntensity
    private float noiseSeed;

    // ─────────────────────────────────────────
    // COLOR SHIFT
    // ─────────────────────────────────────────
    [Header("Color Shift (opcional)")]
    public bool useColorShift = false;
    public Color stableColor = Color.white;
    public Color flickerColor = new Color(1f, 0.85f, 0.6f); // cálido al corte
    [Range(0f, 1f)] public float colorShiftIntensity = 0.5f;

    // ─────────────────────────────────────────
    // ESTADO INTERNO
    // ─────────────────────────────────────────
    private static System.Collections.Generic.Dictionary<int, float> syncTimers
        = new System.Collections.Generic.Dictionary<int, float>();

    private Coroutine flickerCoroutine;
    private bool isMalfunctioning = false;

    // ─────────────────────────────────────────

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (targetLight == null)
        {
            Debug.LogWarning($"[HorrorFlickerLight] No se encontró Light en {gameObject.name}.");
            enabled = false;
            return;
        }

        baseIntensity = targetLight.intensity;
        noiseSeed = Random.Range(0f, 100f);
    }

    private void OnEnable()
    {
        StartFlicker();
    }

    private void OnDisable()
    {
        StopFlicker();
        if (targetLight != null)
        {
            targetLight.intensity = baseIntensity;
            if (useColorShift) targetLight.color = stableColor;
        }
    }

    public void StartFlicker()
    {
        StopFlicker();
        flickerCoroutine = StartCoroutine(FlickerLoop());
    }

    public void StopFlicker()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }
    }

    // API pública para forzar malfunción desde otro script
    public void TriggerMalfunction(float duration)
    {
        StartCoroutine(ForcedMalfunction(duration));
    }

    // ─────────────────────────────────────────
    // LOOP PRINCIPAL
    // ─────────────────────────────────────────

    private IEnumerator FlickerLoop()
    {
        if (syncPhaseOffset > 0f)
            yield return new WaitForSeconds(syncPhaseOffset);

        while (true)
        {
            switch (mode)
            {
                case FlickerMode.Random:
                    yield return StartCoroutine(RandomFlicker());
                    break;
                case FlickerMode.Rhythmic:
                    yield return StartCoroutine(RhythmicFlicker());
                    break;
                case FlickerMode.Malfunction:
                    yield return StartCoroutine(MalfunctionFlicker());
                    break;
                case FlickerMode.Noise:
                    yield return StartCoroutine(NoiseFlicker());
                    break;
            }
        }
    }

    // ─────────────────────────────────────────
    // RANDOM
    // ─────────────────────────────────────────

    private IEnumerator RandomFlicker()
    {
        // esperar período estable
        float wait = useSyncGroup
            ? GetSyncWait()
            : Random.Range(randomEventIntervalMin, randomEventIntervalMax);

        yield return new WaitForSeconds(wait);

        int flickerCount = Random.Range(flickersPerEventMin, flickersPerEventMax + 1);

        for (int i = 0; i < flickerCount; i++)
        {
            SetIntensity(Random.Range(minIntensityRatio, maxIntensityRatio));
            yield return new WaitForSeconds(Random.Range(randomOffTimeMin, randomOffTimeMax));

            SetIntensity(maxIntensityRatio);
            yield return new WaitForSeconds(Random.Range(randomOnTimeMin, randomOnTimeMax));
        }

        SetIntensity(maxIntensityRatio);
    }

    // ─────────────────────────────────────────
    // RHYTHMIC
    // ─────────────────────────────────────────

    private IEnumerator RhythmicFlicker()
    {
        int cycles = rhythmBurstCount > 0 ? rhythmBurstCount : int.MaxValue;

        for (int i = 0; i < cycles; i++)
        {
            SetIntensity(minIntensityRatio);
            yield return new WaitForSeconds(rhythmOffDuration);

            SetIntensity(maxIntensityRatio);
            yield return new WaitForSeconds(rhythmOnDuration);
        }

        if (rhythmBurstCount > 0)
        {
            SetIntensity(maxIntensityRatio);
            yield return new WaitForSeconds(Random.Range(rhythmPauseMin, rhythmPauseMax));
        }
    }

    // ─────────────────────────────────────────
    // MALFUNCTION
    // ─────────────────────────────────────────

    private IEnumerator MalfunctionFlicker()
    {
        // período estable
        isMalfunctioning = false;
        SetIntensity(maxIntensityRatio);
        yield return new WaitForSeconds(Random.Range(stableTimeMin, stableTimeMax));

        // ráfaga de malfunción
        isMalfunctioning = true;
        float malfDuration = Random.Range(malfunctionTimeMin, malfunctionTimeMax);
        float elapsed = 0f;

        while (elapsed < malfDuration)
        {
            float ratio = Random.value > 0.5f
                ? Random.Range(minIntensityRatio, malfunctionBaseRatio)
                : malfunctionBaseRatio;

            SetIntensity(ratio);

            float stepTime = Random.Range(0.02f, 0.1f);
            yield return new WaitForSeconds(stepTime);
            elapsed += stepTime;
        }

        isMalfunctioning = false;
        SetIntensity(maxIntensityRatio);
    }

    private IEnumerator ForcedMalfunction(float duration)
    {
        isMalfunctioning = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            SetIntensity(Random.Range(minIntensityRatio, malfunctionBaseRatio));
            float step = Random.Range(0.02f, 0.08f);
            yield return new WaitForSeconds(step);
            elapsed += step;
        }

        SetIntensity(maxIntensityRatio);
        isMalfunctioning = false;
    }

    // ─────────────────────────────────────────
    // NOISE
    // ─────────────────────────────────────────

    private IEnumerator NoiseFlicker()
    {
        while (mode == FlickerMode.Noise)
        {
            float n = Mathf.PerlinNoise(noiseSeed + Time.time * noiseSpeed, 0f);
            float ratio = Mathf.Lerp(1f - noiseIntensity, 1f, n);
            ratio = Mathf.Clamp(ratio, minIntensityRatio, maxIntensityRatio);
            SetIntensity(ratio);
            yield return null;
        }
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────

    private void SetIntensity(float ratio)
    {
        if (targetLight == null) return;
        targetLight.intensity = baseIntensity * ratio;

        if (useColorShift)
            targetLight.color = Color.Lerp(stableColor, flickerColor,
                                           (1f - ratio) * colorShiftIntensity);
    }

    private float GetSyncWait()
    {
        if (!syncTimers.ContainsKey(syncGroup))
            syncTimers[syncGroup] = Random.Range(randomEventIntervalMin, randomEventIntervalMax);

        return syncTimers[syncGroup];
    }
}