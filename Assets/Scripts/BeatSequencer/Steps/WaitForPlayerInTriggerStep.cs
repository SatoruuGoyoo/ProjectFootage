using UnityEngine;

public class WaitForPlayerInTriggerStep : SequenceStep
{
    [SerializeField] private TriggerZone triggerZone;

    protected override void OnExecute()
    {
        if (triggerZone == null)
        {
            Debug.LogWarning("[WaitForPlayerInTriggerStep] triggerZone no asignado, completando.");
            Complete();
            return;
        }

        if (triggerZone.PlayerInside)
        {
            Complete();
            return;
        }

        triggerZone.OnPlayerEntered += HandlePlayerEntered;
    }

    private void HandlePlayerEntered()
    {
        triggerZone.OnPlayerEntered -= HandlePlayerEntered;
        Complete();
    }

    //protected override void OnReset()
    //{
    //    if (triggerZone != null) triggerZone.OnPlayerEntered -= HandlePlayerEntered;
    //}
}