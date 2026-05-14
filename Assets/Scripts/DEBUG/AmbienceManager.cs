using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class AmbienceManager : MonoBehaviour
{
    public static AmbienceManager Instance { get; private set; }

    [SerializeField] private float crossfadeTime = 2f;

    private EventInstance current;
    private EventReference currentRef;

    private void Awake() => Instance = this;

    public void SetAmbience(EventReference newRef)
    {
        if (newRef.Guid == currentRef.Guid) return; // misma zona, no hacer nada

        // Fade out + stop del anterior
        if (current.isValid())
        {
            StartCoroutine(FadeOutAndRelease(current, crossfadeTime));
        }

        // Crear y arrancar el nuevo con fade in
        current = FMODManager.Instance.CreateEventInstance(newRef);
        currentRef = newRef;
        current.start();
        StartCoroutine(FadeIn(current, crossfadeTime));
    }

    private System.Collections.IEnumerator FadeIn(EventInstance instance, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!instance.isValid()) yield break;

            elapsed += Time.deltaTime;
            float vol = Mathf.Clamp01(elapsed / duration);
            instance.setVolume(vol);
            yield return null;
        }

        if (instance.isValid()) instance.setVolume(1f);
    }

    private System.Collections.IEnumerator FadeOutAndRelease(EventInstance instance, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!instance.isValid()) yield break;

            elapsed += Time.deltaTime;
            float vol = Mathf.Clamp01(1f - (elapsed / duration));
            instance.setVolume(vol);
            yield return null;
        }

        if (instance.isValid())
        {
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
        }
    }
}