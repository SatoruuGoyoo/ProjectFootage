using UnityEngine;

public class PlayerSpeakStep : SequenceStep
{
    [SerializeField] private PlayerVoiceLine line;

    protected override void OnExecute()
    {
        if (PlayerVoicePlayer.Instance == null)
        {
            Debug.LogWarning("[PlayerSpeakStep] No hay PlayerVoicePlayer en escena.");
            Complete();
            return;
        }

        PlayerVoicePlayer.Instance.Play(line, Complete);
    }
}