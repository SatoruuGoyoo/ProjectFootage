using UnityEngine;

public class LockCamcorderInputs : SequenceStep
{
    [SerializeField] private bool blockLift = true;
    [SerializeField] private bool blockRecord = true;
    [SerializeField] private bool blockMenu = true;

    protected override void OnExecute()
    {
        if (blockLift)
        {
            CamcorderController.LiftInputBlocked = true;
        }
        if (blockRecord)
        {
            CamcorderController.RecordInputBlocked = true;
        }
        if (blockMenu)
        {
            CamcorderMenuController.MenuInputBlocked = true;
        }

        Complete();


    }
}
