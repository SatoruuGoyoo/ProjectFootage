using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayFMODSoundStep : SequenceStep
{
    [SerializeField] private EventReference soundEvent;
    [SerializeField] private bool waitForFinish = false;
    [SerializeField] private Transform soundPosition;

    protected override void OnExecute()
    {
        if (soundEvent.IsNull)
        {
            Complete();
            return;
        }

        Vector3 pos = soundPosition != null ? soundPosition.position : transform.position;

        if (!waitForFinish)
        {
            RuntimeManager.PlayOneShot(soundEvent, pos);
            Complete();
            return;
        }

        StartCoroutine(PlayAndWait(pos));
    }

    private IEnumerator PlayAndWait(Vector3 pos)
    {
        var instance = RuntimeManager.CreateInstance(soundEvent);
        instance.set3DAttributes(RuntimeUtils.To3DAttributes(pos));
        instance.start();

        PLAYBACK_STATE state;
        do
        {
            yield return null;
            instance.getPlaybackState(out state);
        } while (state != PLAYBACK_STATE.STOPPED);

        instance.release();
        Complete();
    }
}