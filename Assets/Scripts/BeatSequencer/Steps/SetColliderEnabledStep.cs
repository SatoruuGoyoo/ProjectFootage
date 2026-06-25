using UnityEngine;

public class SetColliderEnabledStep : SequenceStep
{
    [SerializeField] private Collider target;
    [SerializeField] private bool enabledState = true;

    protected override void OnExecute()
    {
        if (target != null) target.enabled = enabledState;
        Complete();
    }
}