using UnityEngine;

public abstract class SequenceStep : MonoBehaviour
{
   public bool IsRunning { get; private set; }
   public bool IsCompleted { get; private set; }

    public void Execute()
    {
         if (IsRunning || IsCompleted) return;
         IsRunning = true;
         IsCompleted = false;
         OnExecute();
    }

    protected abstract void OnExecute();

    protected void Complete()
    {
        if (!IsRunning) return;
        IsRunning = false;
        IsCompleted = true;
    }

    //public void ResetStep()
    //{
    //    IsRunning = false;
    //    IsCompleted = false;
    //    OnReset();
    //}

   // protected abstract void OnReset();
}
