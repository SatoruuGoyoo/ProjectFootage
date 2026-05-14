using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Rendering;

public class SceneAmbience : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference ambienceEvent;

    [Header("Audio Settings")]
    [SerializeField] private float ambienceVolume = 1f;
    [SerializeField] private bool persistAcrossScenes = true;

    private EventInstance ambienceInstance;

    private void Awake()
    {
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ambienceInstance = FMODManager.Instance.CreateEventInstance(ambienceEvent);
        ambienceInstance.setVolume(ambienceVolume);
        ambienceInstance.start();
    }

    private void OnDestroy()
    {
        ambienceInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ambienceInstance.release();
    }

    // In case you want to change volume in runtime from other script
    private void SetVolume(float newVolume)
    {
        ambienceVolume = Mathf.Clamp01(newVolume);
        ambienceInstance.setVolume(ambienceVolume);
    }
}
