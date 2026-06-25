using UnityEngine;

public class BlockSprintStep : SequenceStep
{
    protected override void OnExecute()
    {
        PlayerInput.SprintBlocked = true;
        Complete();
    }
}