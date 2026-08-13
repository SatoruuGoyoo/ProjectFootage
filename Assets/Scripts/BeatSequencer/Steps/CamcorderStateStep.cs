using UnityEngine;

public class CamcorderStateStep : SequenceStep
{
    private enum Action { Lower, Raise }

    [SerializeField] private CamcorderController camcorderController;
    [SerializeField] private Action action = Action.Lower;

    protected override void OnExecute()
    {
        if (camcorderController != null)
        {
            if (action == Action.Lower)
                camcorderController.ForceLowerCamera();
            else
                camcorderController.ForceRaiseCamera();
        }

        Complete();
    }
}
