using UnityEngine;
using FMODUnity;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerMotor playerMotor;

    [Header("Footstep SFX")]
    [SerializeField] private EventReference walkEvent;
    [SerializeField] private EventReference runEvent;

    public void PlayFootstep()
    {
        bool sprinting = playerMotor != null && playerMotor.IsSprinting;
        EventReference evt = sprinting ? runEvent : walkEvent;
        if (evt.IsNull) return;

        RuntimeManager.PlayOneShot(evt, transform.position);
    }
}