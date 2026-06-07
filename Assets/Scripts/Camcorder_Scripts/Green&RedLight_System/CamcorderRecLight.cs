using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class CamcorderRecLight : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Light recordingLight;

    [Header("Colors")]
    [SerializeField] private Color objectiveColor = Color.green;
    [SerializeField] private Color deadZoneColor = Color.red;
    [SerializeField] private Color noneColor = Color.yellow;

    [Header("Blink")]
    [Tooltip("Blink speed when in Objective zone (green). 0 = no blink.")]
    [SerializeField] private float objectiveBlinkSpeed = 2f;
    [Tooltip("Blink speed when in DeadZone (red). 0 = no blink.")]
    [SerializeField] private float deadZoneBlinkSpeed = 0f;
    [Tooltip("Blink speed when no target (yellow). 0 = no blink.")]
    [SerializeField] private float noneBlinkSpeed = 0f;

    [Header("FMOD")]
    [SerializeField] private EventReference recLightEvent;

    [Header("Beep — Objective (Green)")]
    [SerializeField] private int objectiveBeepsPerBurst = 3;
    [SerializeField] private float objectiveBeepInterval = 0.3f;
    [SerializeField] private float objectiveBurstCooldown = 2f;

    [Header("Beep — DeadZone (Red)")]
    [SerializeField] private int deadZoneBeepsPerBurst = 1;
    [SerializeField] private float deadZoneBeepInterval = 0.3f;
    [SerializeField] private float deadZoneBurstCooldown = 5f;

    [Header("Beep — None (Yellow)")]
    [SerializeField] private int noneBeepsPerBurst = 0;
    [SerializeField] private float noneBeepInterval = 0.5f;
    [SerializeField] private float noneBurstCooldown = 10f;

    private EventInstance recLightInstance;
    private CamcorderZone _currentZone = CamcorderZone.None;
    private bool _isActive = false;

    private int _beepsRemainingInBurst;
    private float _beepTimer;
    private float _waitForNext;

    private int CurrentBeepsPerBurst => _currentZone switch
    {
        CamcorderZone.Objective => objectiveBeepsPerBurst,
        CamcorderZone.DeadZone => deadZoneBeepsPerBurst,
        _ => noneBeepsPerBurst
    };

    private float CurrentBeepInterval => _currentZone switch
    {
        CamcorderZone.Objective => objectiveBeepInterval,
        CamcorderZone.DeadZone => deadZoneBeepInterval,
        _ => noneBeepInterval
    };

    private float CurrentBurstCooldown => _currentZone switch
    {
        CamcorderZone.Objective => objectiveBurstCooldown,
        CamcorderZone.DeadZone => deadZoneBurstCooldown,
        _ => noneBurstCooldown
    };

    private Color CurrentColor => _currentZone switch
    {
        CamcorderZone.Objective => objectiveColor,
        CamcorderZone.DeadZone => deadZoneColor,
        _ => noneColor
    };

    private float CurrentBlinkSpeed => _currentZone switch
    {
        CamcorderZone.Objective => objectiveBlinkSpeed,
        CamcorderZone.DeadZone => deadZoneBlinkSpeed,
        _ => noneBlinkSpeed
    };

    private void OnEnable()
    {
        GameEvents.OnZoneChanged += OnZoneChanged;
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;

        recordingLight.enabled = false;
    }

    private void OnDisable()
    {
        GameEvents.OnZoneChanged -= OnZoneChanged;
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    }

    private void Start()
    {
        recLightInstance = FMODManager.Instance.CreateEventInstance(recLightEvent);
    }

    private void OnDestroy()
    {
        recLightInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        recLightInstance.release();
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        bool newActive = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;
        if (newActive == _isActive) return;

        _isActive = newActive;

        if (!_isActive)
        {
            recordingLight.enabled = false;
            recLightInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            recordingLight.enabled = true;
            ApplyState();
            StartBurst();
        }
    }

    private void OnZoneChanged(CamcorderZone zone)
    {
        if (_currentZone == zone) return;
        _currentZone = zone;
        ApplyState();

        if (_isActive) StartBurst();
    }

    private void Update()
    {
        if (!_isActive) return;

        float blinkSpeed = CurrentBlinkSpeed;
        if (blinkSpeed > 0f)
        {
            float blink = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI);
            recordingLight.enabled = blink > 0f;
        }
        else
        {
            recordingLight.enabled = true;
        }

        if (CurrentBeepsPerBurst <= 0) return;

        _beepTimer += Time.deltaTime;
        if (_beepTimer < _waitForNext) return;

        _beepTimer = 0f;

        if (_beepsRemainingInBurst <= 0)
            _beepsRemainingInBurst = CurrentBeepsPerBurst;

        recLightInstance.start();
        _beepsRemainingInBurst--;

        _waitForNext = _beepsRemainingInBurst > 0 ? CurrentBeepInterval : CurrentBurstCooldown;
    }

    private void StartBurst()
    {
        if (CurrentBeepsPerBurst <= 0) return;

        recLightInstance.start();
        _beepsRemainingInBurst = CurrentBeepsPerBurst - 1;
        _beepTimer = 0f;
        _waitForNext = _beepsRemainingInBurst > 0 ? CurrentBeepInterval : CurrentBurstCooldown;
    }

    private void ApplyState()
    {
        recordingLight.color = CurrentColor;
        if (CurrentBlinkSpeed <= 0f && _isActive) recordingLight.enabled = true;

        float stateParam = _currentZone switch
        {
            CamcorderZone.Objective => 1f,
            CamcorderZone.DeadZone => 0f,
            _ => 2f
        };
        recLightInstance.setParameterByName("State", stateParam);
    }
}