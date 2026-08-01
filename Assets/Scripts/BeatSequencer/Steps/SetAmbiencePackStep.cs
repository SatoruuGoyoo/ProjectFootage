using UnityEngine;

public class SetAmbiencePackStep : SequenceStep
{
    [SerializeField] private AmbiencePack pack;

    protected override void OnExecute()
    {
        if (pack != null && RNGAmbienceManager.Instance != null)
            RNGAmbienceManager.Instance.SetPack(pack);

        Complete();
    }
}