using UnityEngine;
using FMODUnity;

public class ElevatorArrival : MonoBehaviour
{
    [SerializeField] private EventReference arrivalSound;

    public void PlayArrival()
    {
        if (arrivalSound.IsNull) return;
        RuntimeManager.PlayOneShot(arrivalSound, transform.position);
    }
}