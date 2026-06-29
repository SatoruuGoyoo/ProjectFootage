using System.Collections;
using UnityEngine;
using FMODUnity;

public class SetFMODBusVolumeStep : SequenceStep
{
    [SerializeField] private string busPath = "bus:/";
    [SerializeField][Range(0f, 1f)] private float targetVolume = 0f;
    [SerializeField] private float fadeDuration = 0f;

    protected override void OnExecute()
    {
        FMOD.Studio.Bus bus = RuntimeManager.GetBus(busPath);

        if (!bus.isValid())
        {
            Debug.LogWarning($"[SetFMODBusVolumeStep] Bus no encontrado: '{busPath}'. Verificá el path en FMOD Studio.");
            Complete();
            return;
        }

        if (fadeDuration <= 0f)
        {
            bus.setVolume(targetVolume);
            Complete();
        }
        else
        {
            StartCoroutine(Fade(bus));
        }
    }

    private IEnumerator Fade(FMOD.Studio.Bus bus)
    {
        bus.getVolume(out float startVolume);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            bus.setVolume(Mathf.Lerp(startVolume, targetVolume, t));
            yield return null;
        }

        bus.setVolume(targetVolume);
        Complete();
    }
}