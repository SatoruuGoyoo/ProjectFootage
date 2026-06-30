using UnityEngine;

public class WaitForUnityEventStep : SequenceStep
{
    protected override void OnExecute() { }

    public void NotifyCompleted()
    {
        Complete();
    }
}