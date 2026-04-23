//using UnityEditor.Rendering;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Audio;

public class CamcorderRecLight : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private Light recordingLight;
    //[SerializeField] private AudioSource camAudio;

    [Header("Red Light Config")]
    [SerializeField] private Color redColor = Color.red;
   // [SerializeField] AudioClip redClip;

    [Header("Green Light Config")]
    [SerializeField] private Color greenColor = Color.green;
   // [SerializeField] AudioClip greenClip;
    [SerializeField] private float blinkSpeed = 2f;

    [Header("FMOD")]
    [SerializeField] private EventReference recLightEvent;

    private EventInstance recLightInstance;
    private bool isGreen = false;
    private bool isActive = false;

    private void OnEnable()
    {
        GameEvents.OnCamcorderLightChanged += OnLightChanged;
        GameEvents.OnPlayerModeChanged += OnPlayerModeChanged;
    }
    private void OnDisable()
    {
        GameEvents.OnCamcorderLightChanged -= OnLightChanged;
        GameEvents.OnPlayerModeChanged -= OnPlayerModeChanged;
    }

    private void Start()
    {
        // Create once ( persists until manually stopped or destroyed )
        recLightInstance = FMODManager.Instance.CreateEventInstance(recLightEvent);
    }

    private void OnDestroy()
    {
        recLightInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        recLightInstance.release();
    }

    private void OnPlayerModeChanged(PlayerMode newMode)
    {
        isActive = newMode == PlayerMode.CameraMode || newMode == PlayerMode.RecordingMode;

        if (!isActive)
        {
            recordingLight.enabled = false;
            recLightInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            recordingLight.enabled = true;
            recLightInstance.start();
            ApplyState();
        }
    }

    private void OnLightChanged(bool isGreen)
    {
        this.isGreen = isGreen;
        ApplyState();
    }

    private void Update()
    {
        if (!isActive || !isGreen) return;

        // Blink the light using a sine wave
        float blink = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI);
        recordingLight.enabled = blink > 0f;
    }

    private void ApplyState()
    {
        recordingLight.color = isGreen ? greenColor : redColor;
        if (!isGreen) recordingLight.enabled = true;

        // Parameter drives everything in FMOD Studio — 0=red, 1=green
        recLightInstance.setParameterByName("State", isGreen ? 1f : 0f);
    }

    //private void PlayAudio(AudioClip clip)
    //{
    //    if (camAudio == null || clip == null) return;
    //    if (camAudio.isPlaying && camAudio.clip == clip) return;
    //    camAudio.clip = clip;
    //    camAudio.loop = true;
    //    camAudio.Play();
    //}

}
