using UnityEngine;

public class UnlockCamcorderInputsStep : SequenceStep
{
    [SerializeField] private bool unblockLift = true;
    [SerializeField] private bool unblockRecord = true;
    [SerializeField] private bool unblockMenu = true;

    protected override void OnExecute()
    {
        if (unblockLift) 
        {
            CamcorderController.LiftInputBlocked = false;
        } 
        if (unblockRecord)
        {
            CamcorderController.RecordInputBlocked = false;
        }
        if (unblockMenu)
        {
            CamcorderMenuController.MenuInputBlocked = false;
        }
        Complete();
    }
}