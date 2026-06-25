using UnityEngine;
using UnityEngine.Events;

public class WaitForUnityEventStep : SequenceStep
{
    [Tooltip("UnityEvent al que suscribirse. Arrastrá un componente y elegí el evento.")]
    [SerializeField] private UnityEvent targetEvent;

    protected override void OnExecute()
    {
        if (targetEvent == null)
        {
            Debug.LogWarning("[WaitForUnityEventStep] targetEvent no asignado, completando.");
            Complete();
            return;
        }

        targetEvent.AddListener(HandleEvent);
    }

    private void HandleEvent()
    {
        targetEvent.RemoveListener(HandleEvent);
        Complete();
    }

    //protected override void OnReset()
    //{
    //    if (targetEvent != null) targetEvent.RemoveListener(HandleEvent);
    //}
}