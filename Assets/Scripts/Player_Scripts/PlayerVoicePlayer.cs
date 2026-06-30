using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerVoicePlayer : MonoBehaviour
{
    public static PlayerVoicePlayer Instance { get; private set; }

    [SerializeField] private SubtitleBlock subtitleBlock;

    private Coroutine _runningRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Coroutine Play(PlayerVoiceLine line, System.Action onComplete = null)
    {
        if (_runningRoutine != null) StopCoroutine(_runningRoutine);
        _runningRoutine = StartCoroutine(SpeakRoutine(line, onComplete));
        return _runningRoutine;
    }

    private IEnumerator SpeakRoutine(PlayerVoiceLine line, System.Action onComplete)
    {
        if (subtitleBlock != null && !string.IsNullOrEmpty(line.text))
            subtitleBlock.Show(line.text);

        if (line.audioEvent.IsNull)
        {
            yield return new WaitForSeconds(line.displayDurationNoAudio);
        }
        else
        {
            var instance = RuntimeManager.CreateInstance(line.audioEvent);
            instance.start();

            PLAYBACK_STATE state;
            do
            {
                yield return null;
                instance.getPlaybackState(out state);
            } while (state != PLAYBACK_STATE.STOPPED);

            instance.release();

            if (line.graceTimeAfterAudio > 0f)
                yield return new WaitForSeconds(line.graceTimeAfterAudio);
        }

        if (subtitleBlock != null) subtitleBlock.Hide();
        _runningRoutine = null;
        onComplete?.Invoke();
    }
}