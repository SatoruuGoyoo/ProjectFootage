using UnityEngine;

public class UnblockSprintStep : SequenceStep
{
    protected override void OnExecute()
    {
        PlayerInput.SprintBlocked = false;
        Complete();
    }
}