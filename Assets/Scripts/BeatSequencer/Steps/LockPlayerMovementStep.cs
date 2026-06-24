using UnityEngine;

public class LockPlayerMovementStep : SequenceStep
{
    [SerializeField] private PlayerMotor playerMotor;

    protected override void OnExecute()
    {
        PlayerController.MovementBlocked = true;
        Debug.Log($"[LockPlayerMovementStep] MovementBlocked seteado en TRUE. Valor actual: {PlayerController.MovementBlocked}");
        if (playerMotor != null) playerMotor.StopPlayer();
        Complete();
    }
}