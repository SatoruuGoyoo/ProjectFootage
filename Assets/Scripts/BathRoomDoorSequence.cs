using UnityEngine;

public class BathroomDoorSequence : MonoBehaviour
{
    [SerializeField] private Door door;
    [SerializeField] private string showerEventId = "";

    [Tooltip("true = abre al completar la grabación. false = abre al VER la grabación.")]
    [SerializeField] private bool openOnComplete = false;

    private void OnEnable()
    {
        GameEvents.OnCamcorderPickedUp += HandlePickedUp;
        if (openOnComplete)
            GameEvents.OnRecordableEventCompleted += HandleTrigger;
        else
            GameEvents.OnRecordableEventWatched += HandleTrigger;
    }

    private void OnDisable()
    {
        GameEvents.OnCamcorderPickedUp -= HandlePickedUp;
        GameEvents.OnRecordableEventCompleted -= HandleTrigger;
        GameEvents.OnRecordableEventWatched -= HandleTrigger;
    }

    private void HandlePickedUp()
    {
        if (door == null) return;
        door.Close();
        door.Lock();
    }

    private void HandleTrigger(string id)
    {
        if (id != showerEventId || door == null) return;
        door.Unlock();
        door.Open();
    }
}