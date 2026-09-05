using UnityEngine;
using System.Collections;

public class HorrorShake : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // MODO
    // ═══════════════════════════════════════════
    [Header("Modo")]
    public ShakeMode mode = ShakeMode.OnDemand;

    public enum ShakeMode
    {
        OnDemand,   // solo agita cuando se llama por código
        Loop,       // agita constantemente
        Interval,   // agita cada cierto tiempo (puerta que golpea sola)
    }

    // ═══════════════════════════════════════════
    // TIPO DE SHAKE
    // ═══════════════════════════════════════════
    [Header("Tipo de Shake")]
    public ShakeType shakeType = ShakeType.Noise;

    public enum ShakeType
    {
        Noise,   // Perlin noise, orgánico y fluido
        Random,  // cortes abruptos, más violento
        Spring,  // rebote con retorno a origen, ideal para puertas
    }

    // ═══════════════════════════════════════════
    // CANALES
    // ═══════════════════════════════════════════
    [Header("Canales activos")]
    public bool shakePosition = false;
    public bool shakeRotation = true;
    public bool shakeScale = false;

    // ═══════════════════════════════════════════
    // POSICIÓN
    // ═══════════════════════════════════════════
    [Header("Posición")]
    public Vector3 positionAmplitude = new Vector3(0.02f, 0f, 0.02f);
    public float positionFrequency = 20f;
    [Tooltip("Ejes bloqueados en posición.")]
    public bool lockPosX = false;
    public bool lockPosY = true;
    public bool lockPosZ = false;

    // ═══════════════════════════════════════════
    // ROTACIÓN
    // ═══════════════════════════════════════════
    [Header("Rotación")]
    public Vector3 rotationAmplitude = new Vector3(0f, 3f, 2f);
    public float rotationFrequency = 18f;
    [Tooltip("Ejes bloqueados en rotación.")]
    public bool lockRotX = false;
    public bool lockRotY = false;
    public bool lockRotZ = false;

    // ═══════════════════════════════════════════
    // ESCALA
    // ═══════════════════════════════════════════
    [Header("Escala")]
    public Vector3 scaleAmplitude = new Vector3(0.02f, 0.02f, 0.02f);
    public float scaleFrequency = 15f;
    public bool uniformScale = true;

    // ═══════════════════════════════════════════
    // INTENSIDAD GENERAL
    // ═══════════════════════════════════════════
    [Header("Intensidad")]
    [Range(0f, 1f)] public float intensity = 1f;
    [Tooltip("Multiplicador de intensidad en ráfagas breves (TriggerBurst).")]
    public float burstMultiplier = 2.5f;

    // ═══════════════════════════════════════════
    // DECAY / FADE
    // ═══════════════════════════════════════════
    [Header("Decay (solo OnDemand)")]
    [Tooltip("Si true, el shake decae hasta cero solo.")]
    public bool autoDecay = true;
    public float decayDuration = 0.6f;
    public AnimationCurve decayCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    // ═══════════════════════════════════════════
    // SPRING
    // ═══════════════════════════════════════════
    [Header("Spring (ShakeType.Spring)")]
    [Tooltip("Qué tan fuerte tira de vuelta al origen.")]
    public float springStiffness = 200f;
    [Tooltip("Amortiguación del rebote.")]
    public float springDamping = 20f;
    [Tooltip("Impulso inicial al llamar Play().")]
    public Vector3 springImpulse = new Vector3(0f, 5f, 3f);

    // ═══════════════════════════════════════════
    // INTERVAL MODE
    // ═══════════════════════════════════════════
    [Header("Modo Interval")]
    public float intervalMin = 1.5f;
    public float intervalMax = 4f;
    public float intervalShakeduration = 0.4f;

    // ═══════════════════════════════════════════
    // RUIDO PERLIN
    // ═══════════════════════════════════════════
    [Header("Noise")]
    [Tooltip("Semilla aleatoria al Start. Podés fijarla para comportamiento determinístico.")]
    public bool randomSeed = true;
    public float seed = 0f;

    // ═══════════════════════════════════════════
    // ESTADO INTERNO
    // ═══════════════════════════════════════════
    private Vector3 originPos;
    private Quaternion originRot;
    private Vector3 originScale;

    private float currentIntensity = 0f;
    private float noiseTime = 0f;

    // spring
    private Vector3 springVelocityPos;
    private Vector3 springVelocityRot;
    private Vector3 springOffsetPos;
    private Vector3 springOffsetRot;

    private Coroutine decayCoroutine;
    private Coroutine intervalCoroutine;
    private bool isPlaying = false;

    // ═══════════════════════════════════════════
    // INIT
    // ═══════════════════════════════════════════

    private void Awake()
    {
        CacheOrigin();
        if (randomSeed) seed = Random.Range(0f, 999f);
    }

    private void OnEnable()
    {
        CacheOrigin();

        if (mode == ShakeMode.Loop)
        {
            currentIntensity = 1f;
            isPlaying = true;
        }
        else if (mode == ShakeMode.Interval)
        {
            intervalCoroutine = StartCoroutine(IntervalLoop());
        }
    }

    private void OnDisable()
    {
        ResetTransform();
        isPlaying = false;
        if (intervalCoroutine != null) StopCoroutine(intervalCoroutine);
    }

    private void CacheOrigin()
    {
        originPos = transform.localPosition;
        originRot = transform.localRotation;
        originScale = transform.localScale;
    }

    // ═══════════════════════════════════════════
    // UPDATE
    // ═══════════════════════════════════════════

    private void Update()
    {
        if (!isPlaying && shakeType != ShakeType.Spring) return;
        if (shakeType == ShakeType.Spring) { UpdateSpring(); return; }

        noiseTime += Time.deltaTime;
        ApplyShake(currentIntensity);
    }

    // ═══════════════════════════════════════════
    // APPLY
    // ═══════════════════════════════════════════

    private void ApplyShake(float intensityMultiplier)
    {
        float t = noiseTime;
        float i = intensity * intensityMultiplier;

        if (shakePosition)
        {
            Vector3 offset = EvaluateVector(positionAmplitude, positionFrequency, t, 0f) * i;
            if (lockPosX) offset.x = 0f;
            if (lockPosY) offset.y = 0f;
            if (lockPosZ) offset.z = 0f;
            transform.localPosition = originPos + offset;
        }

        if (shakeRotation)
        {
            Vector3 euler = EvaluateVector(rotationAmplitude, rotationFrequency, t, 100f) * i;
            if (lockRotX) euler.x = 0f;
            if (lockRotY) euler.y = 0f;
            if (lockRotZ) euler.z = 0f;
            transform.localRotation = originRot * Quaternion.Euler(euler);
        }

        if (shakeScale)
        {
            Vector3 scaleDelta = EvaluateVector(scaleAmplitude, scaleFrequency, t, 200f) * i;
            if (uniformScale)
            {
                float uniform = (scaleDelta.x + scaleDelta.y + scaleDelta.z) / 3f;
                scaleDelta = Vector3.one * uniform;
            }
            transform.localScale = originScale + scaleDelta;
        }
    }

    private Vector3 EvaluateVector(Vector3 amplitude, float frequency, float t, float seedOffset)
    {
        switch (shakeType)
        {
            case ShakeType.Noise:
                return new Vector3(
                    NoiseAxis(seed + seedOffset + 0f, t, frequency, amplitude.x),
                    NoiseAxis(seed + seedOffset + 50f, t, frequency, amplitude.y),
                    NoiseAxis(seed + seedOffset + 100f, t, frequency, amplitude.z)
                );

            case ShakeType.Random:
            default:
                return new Vector3(
                    Random.Range(-amplitude.x, amplitude.x),
                    Random.Range(-amplitude.y, amplitude.y),
                    Random.Range(-amplitude.z, amplitude.z)
                );
        }
    }

    private float NoiseAxis(float s, float t, float freq, float amp)
    {
        // Perlin centrado en 0
        return (Mathf.PerlinNoise(s, t * freq) - 0.5f) * 2f * amp;
    }

    // ═══════════════════════════════════════════
    // SPRING
    // ═══════════════════════════════════════════

    private void UpdateSpring()
    {
        // posición
        if (shakePosition)
        {
            Vector3 force = -springStiffness * springOffsetPos
                            - springDamping * springVelocityPos;
            springVelocityPos += force * Time.deltaTime;
            springOffsetPos += springVelocityPos * Time.deltaTime;
            transform.localPosition = originPos + springOffsetPos;
        }

        // rotación
        if (shakeRotation)
        {
            Vector3 force = -springStiffness * springOffsetRot
                            - springDamping * springVelocityRot;
            springVelocityRot += force * Time.deltaTime;
            springOffsetRot += springVelocityRot * Time.deltaTime;
            transform.localRotation = originRot * Quaternion.Euler(springOffsetRot);
        }

        // parar cuando todo está quieto
        bool settled = springOffsetPos.magnitude < 0.0001f &&
                       springVelocityPos.magnitude < 0.0001f &&
                       springOffsetRot.magnitude < 0.001f &&
                       springVelocityRot.magnitude < 0.001f;

        if (settled && mode == ShakeMode.OnDemand)
        {
            ResetTransform();
            isPlaying = false;
        }
    }

    // ═══════════════════════════════════════════
    // INTERVAL LOOP
    // ═══════════════════════════════════════════

    private IEnumerator IntervalLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));
            Play(intervalShakeduration);
        }
    }

    // ═══════════════════════════════════════════
    // DECAY COROUTINE
    // ═══════════════════════════════════════════

    private IEnumerator DecayRoutine(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            currentIntensity = decayCurve.Evaluate(elapsed / duration);
            yield return null;
        }

        currentIntensity = 0f;
        isPlaying = false;
        ResetTransform();
    }

    // ═══════════════════════════════════════════
    // API PÚBLICA
    // ═══════════════════════════════════════════

    /// <summary>
    /// Arranca el shake. Si se pasa duration > 0 decae solo (solo en OnDemand).
    /// </summary>
    public void Play(float duration = -1f)
    {
        CacheOrigin();
        isPlaying = true;
        currentIntensity = 1f;

        if (shakeType == ShakeType.Spring)
        {
            springVelocityPos += new Vector3(
                springImpulse.x * Random.Range(-1f, 1f),
                springImpulse.y * Random.Range(-1f, 1f),
                springImpulse.z * Random.Range(-1f, 1f)
            ) * intensity;
            springVelocityRot += new Vector3(
                springImpulse.x * Random.Range(-1f, 1f),
                springImpulse.y * Random.Range(-1f, 1f),
                springImpulse.z * Random.Range(-1f, 1f)
            ) * intensity;
            return;
        }

        if (decayCoroutine != null) StopCoroutine(decayCoroutine);

        float d = duration > 0f ? duration
                : (autoDecay ? decayDuration : -1f);

        if (d > 0f)
            decayCoroutine = StartCoroutine(DecayRoutine(d));
    }

    /// <summary>
    /// Ráfaga breve con multiplicador de intensidad. Ideal para golpes, sustos.
    /// </summary>
    public void TriggerBurst(float duration = 0.25f)
    {
        float prev = intensity;
        intensity *= burstMultiplier;
        Play(duration);
        intensity = prev;
    }

    /// <summary>
    /// Para el shake y resetea la transform.
    /// </summary>
    public void Stop()
    {
        if (decayCoroutine != null) StopCoroutine(decayCoroutine);
        currentIntensity = 0f;
        isPlaying = false;
        springOffsetPos = Vector3.zero;
        springOffsetRot = Vector3.zero;
        springVelocityPos = Vector3.zero;
        springVelocityRot = Vector3.zero;
        ResetTransform();
    }

    /// <summary>
    /// Cambia la intensidad en caliente (útil para animarla desde otro script).
    /// </summary>
    public void SetIntensity(float value)
    {
        currentIntensity = Mathf.Clamp01(value);
    }

    private void ResetTransform()
    {
        transform.localPosition = originPos;
        transform.localRotation = originRot;
        transform.localScale = originScale;
    }

    // preview en editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.05f);
    }
}