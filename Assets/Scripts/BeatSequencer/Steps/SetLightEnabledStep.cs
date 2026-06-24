using UnityEngine;

public class SetLightEnabledStep : SequenceStep
{
    [SerializeField] private Light targetLight;
    [SerializeField] private bool setEnabled = false;

    protected override void OnExecute()
    {
        if (targetLight != null)
        {
            targetLight.enabled = setEnabled;
        }
        Complete();
    }
}