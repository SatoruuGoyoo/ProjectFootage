using UnityEngine;

public class UnlockPlayerMovementStep : SequenceStep
{
    protected override void OnExecute()
    {
        PlayerController.MovementBlocked = false;
        Complete();
    }
}