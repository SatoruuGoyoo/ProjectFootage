using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerSpeakStep : SequenceStep
{
    [Header("Audio (FMOD)")]
    [SerializeField] private EventReference audioEvent;

    [Header("Text")]
    [TextArea(3, 5)]
    [SerializeField] private string text = "";

    [Header("Subtitle UI")]
    [SerializeField] private SubtitleBlock subtitleBlock;

    [Header("Timing")]
    [SerializeField] private float displayDurationNoAudio = 3f;
    [SerializeField] private float graceTimeAfterAudio = 0.5f;

    protected override void OnExecute()
    {
        StartCoroutine(SpeakRoutine());
    }

    private IEnumerator SpeakRoutine()
    {
        if (subtitleBlock != null && !string.IsNullOrEmpty(text))
            subtitleBlock.Show(text);

        if (audioEvent.IsNull)
        {
            yield return new WaitForSeconds(displayDurationNoAudio);
        }
        else
        {
            var instance = RuntimeManager.CreateInstance(audioEvent);
            instance.start();

            PLAYBACK_STATE state;
            do
            {
                yield return null;
                instance.getPlaybackState(out state);
            } while (state != PLAYBACK_STATE.STOPPED);

            instance.release();

            if (graceTimeAfterAudio > 0f)
                yield return new WaitForSeconds(graceTimeAfterAudio);
        }

        if (subtitleBlock != null) subtitleBlock.Hide();
        Complete();
    }
}