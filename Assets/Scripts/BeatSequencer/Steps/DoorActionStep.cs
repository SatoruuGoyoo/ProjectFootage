using UnityEngine;

public class DoorActionStep : SequenceStep
{
    public enum DoorAction
    {
        Open,
        Close,
        Lock,
        Unlock,
        LockAndClose,
        UnlockAndOpen
    }

    [SerializeField] private Door targetDoor;
    [SerializeField] private DoorAction action;

    protected override void OnExecute()
    {
        if (targetDoor == null)
        {
            Complete();
            return;
        }

        switch (action)
        {
            case DoorAction.Open:
                targetDoor.Open();
                break;

            case DoorAction.Close:
                targetDoor.Close();
                break;

            case DoorAction.Lock:
                targetDoor.Lock();
                break;

            case DoorAction.Unlock:
                targetDoor.Unlock();
                break;

            case DoorAction.LockAndClose:
                targetDoor.Close();
                targetDoor.Lock();
                break;

            case DoorAction.UnlockAndOpen:
                targetDoor.Open();
                targetDoor.Unlock();
                break;
        }

        Complete();
    }


} 


    
   
