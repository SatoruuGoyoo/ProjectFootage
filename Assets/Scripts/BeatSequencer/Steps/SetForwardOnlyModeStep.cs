using UnityEngine;

public class SetForwardOnlyModeStep : SequenceStep
{
    [SerializeField] private bool enabled = true;

    protected override void OnExecute()
    {
        PlayerController.ForwardOnlyMode = enabled;
        Complete();
    }
}