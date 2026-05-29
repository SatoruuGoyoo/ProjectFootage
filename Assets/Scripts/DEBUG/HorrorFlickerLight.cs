using UnityEngine;

// ══════════════════════════════════════════════════════════════
//  INTERFACES  (Strategy pattern — cada modo es intercambiable)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Contrato mínimo para cualquier comportamiento de flicker.
/// Implementación pura de datos + lógica, sin MonoBehaviour ni allocs.
/// </summary>
public interface IFlickerBehaviour
{
    /// <summary>Reinicia el estado interno del comportamiento.</summary>
    void Reset(HorrorFlickerConfig cfg);

    /// <summary>
    /// Llamado cada frame. Devuelve el ratio de intensidad [0,1].
    /// deltaTime se pasa explícitamente para evitar acceder a Time.deltaTime
    /// desde múltiples puntos.
    /// </summary>
    float Tick(float deltaTime, float unscaledTime, HorrorFlickerConfig cfg);
}

// ══════════════════════════════════════════════════════════════
//  CONFIG  (Value object — sólo datos, cero comportamiento)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Todos los parámetros de configuración en un struct plano.
/// Se pasa por referencia a los behaviours; evita captura de closures.
/// </summary>
[System.Serializable]
public struct HorrorFlickerConfig
{
    [Header("Intensidad")]
    [Range(0f, 1f)] public float minIntensityRatio;
    [Range(0f, 1f)] public float maxIntensityRatio;

    [Header("Random")]
    public float randomEventIntervalMin;
    public float randomEventIntervalMax;
    public float randomOnTimeMin;
    public float randomOnTimeMax;
    public float randomOffTimeMin;
    public float randomOffTimeMax;
    public int flickersPerEventMin;
    public int flickersPerEventMax;

    [Header("Rhythmic")]
    public float rhythmOnDuration;
    public float rhythmOffDuration;
    public int rhythmBurstCount;      // 0 = infinito
    public float rhythmPauseMin;
    public float rhythmPauseMax;

    [Header("Malfunction")]
    public float stableTimeMin;
    public float stableTimeMax;
    public float malfunctionTimeMin;
    public float malfunctionTimeMax;
    [Range(0f, 1f)] public float malfunctionBaseRatio;

    [Header("Noise")]
    public float noiseSpeed;
    [Range(0f, 1f)] public float noiseIntensity;

    // Valores por defecto razonables
    public static HorrorFlickerConfig Default => new HorrorFlickerConfig
    {
        minIntensityRatio = 0f,
        maxIntensityRatio = 1f,
        randomEventIntervalMin = 1f,
        randomEventIntervalMax = 4f,
        randomOnTimeMin = 0.05f,
        randomOnTimeMax = 0.3f,
        randomOffTimeMin = 0.02f,
        randomOffTimeMax = 0.15f,
        flickersPerEventMin = 2,
        flickersPerEventMax = 7,
        rhythmOnDuration = 0.1f,
        rhythmOffDuration = 0.1f,
        rhythmBurstCount = 0,
        rhythmPauseMin = 1f,
        rhythmPauseMax = 3f,
        stableTimeMin = 2f,
        stableTimeMax = 6f,
        malfunctionTimeMin = 0.5f,
        malfunctionTimeMax = 2f,
        malfunctionBaseRatio = 0.4f,
        noiseSpeed = 1.5f,
        noiseIntensity = 0.4f,
    };
}

// ══════════════════════════════════════════════════════════════
//  BEHAVIOURS  (cada uno es una clase pequeña, sin allocs en Tick)
// ══════════════════════════════════════════════════════════════

/// <summary>Cortes irregulares en ráfagas separadas por períodos estables.</summary>
public sealed class RandomFlickerBehaviour : IFlickerBehaviour
{
    // Estado de la máquina: Waiting → Flickering → (vuelta a Waiting)
    private enum Phase { Waiting, Off, On }

    private Phase _phase;
    private float _timer;
    private int _flickersLeft;
    private float _currentRatio;

    public void Reset(HorrorFlickerConfig cfg)
    {
        _phase = Phase.Waiting;
        _timer = Random.Range(cfg.randomEventIntervalMin, cfg.randomEventIntervalMax);
        _currentRatio = cfg.maxIntensityRatio;
        _flickersLeft = 0;
    }

    public float Tick(float dt, float _, HorrorFlickerConfig cfg)
    {
        _timer -= dt;

        switch (_phase)
        {
            case Phase.Waiting:
                if (_timer <= 0f)
                {
                    _flickersLeft = Random.Range(cfg.flickersPerEventMin, cfg.flickersPerEventMax + 1);
                    EnterOff(cfg);
                }
                break;

            case Phase.Off:
                if (_timer <= 0f) EnterOn(cfg);
                break;

            case Phase.On:
                if (_timer <= 0f)
                {
                    _flickersLeft--;
                    if (_flickersLeft > 0)
                        EnterOff(cfg);
                    else
                        EnterWaiting(cfg);
                }
                break;
        }

        return _currentRatio;
    }

    private void EnterWaiting(HorrorFlickerConfig cfg)
    {
        _phase = Phase.Waiting;
        _timer = Random.Range(cfg.randomEventIntervalMin, cfg.randomEventIntervalMax);
        _currentRatio = cfg.maxIntensityRatio;
    }

    private void EnterOff(HorrorFlickerConfig cfg)
    {
        _phase = Phase.Off;
        _timer = Random.Range(cfg.randomOffTimeMin, cfg.randomOffTimeMax);
        _currentRatio = Random.Range(cfg.minIntensityRatio, cfg.maxIntensityRatio);
    }

    private void EnterOn(HorrorFlickerConfig cfg)
    {
        _phase = Phase.On;
        _timer = Random.Range(cfg.randomOnTimeMin, cfg.randomOnTimeMax);
        _currentRatio = cfg.maxIntensityRatio;
    }
}

/// <summary>Pulso estroboscópico constante con pausas opcionales.</summary>
public sealed class RhythmicFlickerBehaviour : IFlickerBehaviour
{
    private enum Phase { On, Off, Pause }

    private Phase _phase;
    private float _timer;
    private int _cyclesLeft;
    private float _currentRatio;

    public void Reset(HorrorFlickerConfig cfg)
    {
        _phase = Phase.On;
        _timer = cfg.rhythmOnDuration;
        _cyclesLeft = cfg.rhythmBurstCount > 0 ? cfg.rhythmBurstCount : int.MaxValue;
        _currentRatio = cfg.maxIntensityRatio;
    }

    public float Tick(float dt, float _, HorrorFlickerConfig cfg)
    {
        _timer -= dt;
        if (_timer > 0f) return _currentRatio;

        switch (_phase)
        {
            case Phase.On:
                _phase = Phase.Off;
                _timer = cfg.rhythmOffDuration;
                _currentRatio = cfg.minIntensityRatio;
                break;

            case Phase.Off:
                _cyclesLeft--;
                if (_cyclesLeft <= 0 && cfg.rhythmBurstCount > 0)
                {
                    _phase = Phase.Pause;
                    _timer = Random.Range(cfg.rhythmPauseMin, cfg.rhythmPauseMax);
                    _currentRatio = cfg.maxIntensityRatio;
                }
                else
                {
                    _phase = Phase.On;
                    _timer = cfg.rhythmOnDuration;
                    _currentRatio = cfg.maxIntensityRatio;
                }
                break;

            case Phase.Pause:
                _cyclesLeft = cfg.rhythmBurstCount > 0 ? cfg.rhythmBurstCount : int.MaxValue;
                _phase = Phase.On;
                _timer = cfg.rhythmOnDuration;
                _currentRatio = cfg.maxIntensityRatio;
                break;
        }

        return _currentRatio;
    }
}

/// <summary>Períodos estables con ráfagas de malfunción.</summary>
public sealed class MalfunctionFlickerBehaviour : IFlickerBehaviour
{
    private enum Phase { Stable, Malfunction }

    private Phase _phase;
    private float _timer;
    private float _stepTimer;
    private float _currentRatio;

    public void Reset(HorrorFlickerConfig cfg)
    {
        _phase = Phase.Stable;
        _timer = Random.Range(cfg.stableTimeMin, cfg.stableTimeMax);
        _currentRatio = cfg.maxIntensityRatio;
        _stepTimer = 0f;
    }

    public float Tick(float dt, float _, HorrorFlickerConfig cfg)
    {
        _timer -= dt;
        _stepTimer -= dt;

        switch (_phase)
        {
            case Phase.Stable:
                _currentRatio = cfg.maxIntensityRatio;
                if (_timer <= 0f)
                {
                    _phase = Phase.Malfunction;
                    _timer = Random.Range(cfg.malfunctionTimeMin, cfg.malfunctionTimeMax);
                    _stepTimer = 0f;
                }
                break;

            case Phase.Malfunction:
                if (_stepTimer <= 0f)
                {
                    // nuevo valor aleatorio de intensidad reducida
                    _currentRatio = Random.value > 0.5f
                        ? Random.Range(cfg.minIntensityRatio, cfg.malfunctionBaseRatio)
                        : cfg.malfunctionBaseRatio;
                    _stepTimer = Random.Range(0.02f, 0.1f);
                }

                if (_timer <= 0f)
                {
                    _phase = Phase.Stable;
                    _timer = Random.Range(cfg.stableTimeMin, cfg.stableTimeMax);
                    _currentRatio = cfg.maxIntensityRatio;
                }
                break;
        }

        return _currentRatio;
    }
}

/// <summary>Variación suave de intensidad via Perlin noise.</summary>
public sealed class NoiseFlickerBehaviour : IFlickerBehaviour
{
    private float _seed;

    public void Reset(HorrorFlickerConfig _)
    {
        _seed = Random.Range(0f, 100f);
    }

    public float Tick(float _, float unscaledTime, HorrorFlickerConfig cfg)
    {
        float n = Mathf.PerlinNoise(_seed + unscaledTime * cfg.noiseSpeed, 0f);
        float ratio = Mathf.Lerp(1f - cfg.noiseIntensity, 1f, n);
        return Mathf.Clamp(ratio, cfg.minIntensityRatio, cfg.maxIntensityRatio);
    }
}

// ══════════════════════════════════════════════════════════════
//  MONOBEHAVIOUR  (sólo orquestación, zero allocs en Update)
// ══════════════════════════════════════════════════════════════

/// <summary>
/// Controlador de flicker de luz para juegos de horror.
///
/// Diseño:
///   - Strategy pattern para cada modo de flicker.
///   - Update loop puro, sin coroutines → cero allocs por frame.
///   - Config segregada en HorrorFlickerConfig (ISP / SRP).
///   - Behaviours son clases Plain-Old-Object, testeables de forma aislada.
/// </summary>
[RequireComponent(typeof(Light))]
public sealed class HorrorFlickerLight : MonoBehaviour
{
    // ── Modo ─────────────────────────────────
    [Header("Modo de Flicker")]
    [SerializeField] private FlickerMode _mode = FlickerMode.Random;

    public enum FlickerMode { Random, Rhythmic, Malfunction, Noise }

    // ── Config ───────────────────────────────
    [Header("Configuración")]
    [SerializeField] private HorrorFlickerConfig _config = HorrorFlickerConfig.Default;

    // ── Color Shift ──────────────────────────
    [Header("Color Shift (opcional)")]
    [SerializeField] private bool _useColorShift = false;
    [SerializeField] private Color _stableColor = Color.white;
    [SerializeField] private Color _flickerColor = new Color(1f, 0.85f, 0.6f);
    [Range(0f, 1f)]
    [SerializeField] private float _colorShiftIntensity = 0.5f;

    // ── Phase offset (sync sin Dictionary global) ──
    [Header("Sincronización")]
    [SerializeField] private float _syncPhaseOffset = 0f;

    // ── Referencias internas ─────────────────
    private Light _light;
    private float _baseIntensity;
    private IFlickerBehaviour _behaviour;

    // Para el phase offset: simple timer local
    private bool _active;
    private float _phaseTimer;

    // Ratio previo: evita escribir en la luz si no cambió
    private float _lastRatio = -1f;
    private Color _lastColor;

    // ── Behaviours pre-instanciados (sin alloc en cambio de modo) ──
    private readonly RandomFlickerBehaviour _randomBehaviour = new RandomFlickerBehaviour();
    private readonly RhythmicFlickerBehaviour _rhythmicBehaviour = new RhythmicFlickerBehaviour();
    private readonly MalfunctionFlickerBehaviour _malfunctionBehaviour = new MalfunctionFlickerBehaviour();
    private readonly NoiseFlickerBehaviour _noiseBehaviour = new NoiseFlickerBehaviour();

    // ─────────────────────────────────────────

    private void Awake()
    {
        _light = GetComponent<Light>();
        _baseIntensity = _light.intensity;
        _lastColor = _light.color;
    }

    private void OnEnable()
    {
        _phaseTimer = _syncPhaseOffset;
        _active = _syncPhaseOffset <= 0f;

        SetMode(_mode);
    }

    private void OnDisable()
    {
        ApplyRatio(1f);
    }

    private void Update()
    {
        // Phase offset: esperar antes de activar
        if (!_active)
        {
            _phaseTimer -= Time.deltaTime;
            if (_phaseTimer <= 0f) _active = true;
            return;
        }

        float ratio = _behaviour.Tick(Time.deltaTime, Time.time, _config);
        ApplyRatio(ratio);
    }

    // ── API pública ───────────────────────────

    /// <summary>Cambia el modo en caliente sin allocs.</summary>
    public void SetMode(FlickerMode newMode)
    {
        _mode = newMode;
        _behaviour = newMode switch
        {
            FlickerMode.Random => _randomBehaviour,
            FlickerMode.Rhythmic => _rhythmicBehaviour,
            FlickerMode.Malfunction => _malfunctionBehaviour,
            FlickerMode.Noise => _noiseBehaviour,
            _ => _randomBehaviour,
        };
        _behaviour.Reset(_config);
        _lastRatio = -1f; // forzar escritura
    }

    /// <summary>Fuerza una malfunción desde un script externo.</summary>
    public void TriggerMalfunction(float duration)
    {
        // Inyectamos duración temporal sobrescribiendo el behaviour en curso
        var tmp = _config;
        tmp.malfunctionTimeMin = duration;
        tmp.malfunctionTimeMax = duration;
        SetMode(FlickerMode.Malfunction);
        _config = tmp;
    }

    // ── Helpers ───────────────────────────────

    private void ApplyRatio(float ratio)
    {
        // Dirty-check: sólo escribe si el valor cambió (evita set de propiedad cada frame)
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (ratio == _lastRatio) return;
        _lastRatio = ratio;

        _light.intensity = _baseIntensity * ratio;

        if (_useColorShift)
        {
            Color c = Color.Lerp(_stableColor, _flickerColor, (1f - ratio) * _colorShiftIntensity);
            if (c != _lastColor)
            {
                _light.color = c;
                _lastColor = c;
            }
        }
    }
}