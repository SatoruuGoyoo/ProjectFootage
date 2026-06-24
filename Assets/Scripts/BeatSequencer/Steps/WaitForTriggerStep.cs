using UnityEngine;

public class WaitForTriggerStep : SequenceStep
{
    protected override void OnExecute()
    {
    }

    public void TriggerComplete()
    {
        if (!IsRunning) return;
        Complete();
    }
}