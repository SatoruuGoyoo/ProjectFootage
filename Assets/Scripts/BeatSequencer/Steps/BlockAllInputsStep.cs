using UnityEngine;

public class BlockAllInputsStep : SequenceStep
{
    [SerializeField] private bool block = true;

    protected override void OnExecute()
    {
        InputLock.AllBlocked = block;

        PlayerController.MovementBlocked = block;
        PlayerInput.SprintBlocked = block;
        CamcorderController.LiftInputBlocked = block;
        CamcorderController.RecordInputBlocked = block;
        CamcorderMenuController.MenuInputBlocked = block;

        Complete();
    }
}