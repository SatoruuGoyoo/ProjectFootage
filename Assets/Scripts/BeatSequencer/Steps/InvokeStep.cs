using UnityEngine;
using UnityEngine.Events;

public class InvokeStep : SequenceStep
{
    [SerializeField] private UnityEvent onExecute;

    protected override void OnExecute()
    {
        onExecute?.Invoke();
        Complete();
    }
}