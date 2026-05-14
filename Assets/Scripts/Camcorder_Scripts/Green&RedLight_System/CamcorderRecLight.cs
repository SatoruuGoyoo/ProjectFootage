using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;

public class CamcorderRecLight : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Light recordingLight;

    [Header("Red Light Config")]
    [SerializeField] private Color redColor = Color.red;

    [Header("Green Light Config")]
    [SerializeField] private Color greenColor = Color.green;
    [SerializeField] private float blinkSpeed = 2f;

    [Header("FMOD")]
    [SerializeField] private EventReference recLightEvent;

    [Header("Red Beep Config")]
    [SerializeField] private int redBeepsPerBurst = 1;
    [SerializeField] private float redBeepInterval = 0.3f;
    [SerializeField] private float redBurstCooldown = 5f;

    [Header("Green Beep Config")]
    [SerializeField] private int greenBeepsPerBurst = 3;
    [SerializeField] private float greenBeepInterval = 0.3f;
    [SerializeField] private float greenBurstCooldown = 2f;

    private EventInstance recLightInstance;
    private bool isGreen = false;
    private bool isActive = false;

    private int beepsRemainingInBurst;
    private float beepTimer;
    private float waitForNext;

    private int CurrentBeepsPerBurst => isGreen ? greenBeepsPerBurst : redBeepsPerBurst;
    private float CurrentBeepInterval => isGreen ? greenBeepInterval : redBeepInterval;
    private float CurrentBurstCooldown => isGreen ? greenBurstCooldown : redBurstCooldown;

    private void OnEnable()
    {
        GameEvents.OnCamcorderLightChanged += OnLightChanged;
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;

        recordingLight.enabled = false;
    }
    private void OnDisable()
    {
        GameEvents.OnCamcorderLightChanged -= OnLightChanged;
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
        if (newActive == isActive) return;

        isActive = newActive;

        if (!isActive)
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

    private void OnLightChanged(bool isGreen)
    {
        if (this.isGreen == isGreen) return;
        this.isGreen = isGreen;
        ApplyState();

        if (isActive) StartBurst();
    }

    private void Update()
    {
        if (!isActive) return;

        if (isGreen)
        {
            float blink = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI);
            recordingLight.enabled = blink > 0f;
        }

        beepTimer += Time.deltaTime;
        if (beepTimer < waitForNext) return;

        beepTimer = 0f;

        if (beepsRemainingInBurst <= 0)
            beepsRemainingInBurst = CurrentBeepsPerBurst;

        recLightInstance.start();
        beepsRemainingInBurst--;

        waitForNext = beepsRemainingInBurst > 0 ? CurrentBeepInterval : CurrentBurstCooldown;
    }

    private void StartBurst()
    {
        recLightInstance.start();
        beepsRemainingInBurst = CurrentBeepsPerBurst - 1;
        beepTimer = 0f;
        waitForNext = beepsRemainingInBurst > 0 ? CurrentBeepInterval : CurrentBurstCooldown;
    }

    private void ApplyState()
    {
        recordingLight.color = isGreen ? greenColor : redColor;
        if (!isGreen && isActive) recordingLight.enabled = true;
        recLightInstance.setParameterByName("State", isGreen ? 1f : 0f);
    }
}